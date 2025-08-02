using AgenticAI.Models;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Text;
using System.Text.Json;
using UglyToad.PdfPig;

namespace AgenticAI.Functions;

public class ResumeAnalyzerFunction
{
    private readonly ILogger<ResumeAnalyzerFunction> _logger;
    private readonly Kernel _kernel;

    public ResumeAnalyzerFunction(ILogger<ResumeAnalyzerFunction> logger, Kernel kernel)
    {
        _logger = logger;
        _kernel = kernel;
    }

    [Function("ResumeAnalyzer")]
    public async Task<string> Run(
        [BlobTrigger("received-mails/{name}", Connection = "AzureWebJobsStorage")] Stream blobStream,
        string name)
    {
        try
        {
            _logger.LogInformation($"Processing blob: {name}");

            // Step 1: Deserialize the email request
            EmailRequest? emailRequest;
            try
            {
                var jsonContent = await new StreamReader(blobStream).ReadToEndAsync();
                emailRequest = JsonSerializer.Deserialize<EmailRequest>(jsonContent, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (emailRequest == null)
                {
                    _logger.LogError("Failed to deserialize email request");
                    return "Error: Invalid email format";
                }

                if (!emailRequest.Attachments.Any())
                {
                    _logger.LogInformation("No attachments found in email");
                    return "No attachments to process";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing email request");
                return "Error: Failed to parse email";
            }

            // Step 2: Triage with AI - check if this is a job application
            _logger.LogInformation("Analyzing email with AI for job application triage...");
            
            var triageResult = await _kernel.InvokePromptAsync(
                $@"You are an experienced HR assistant. Your task is to analyze an email and determine if it represents a job application or resume submission.

Email Subject: {emailRequest.Subject}
Email Body: {emailRequest.Body}

Instructions:
- Analyze the subject line and body content
- Look for keywords indicating job application, resume submission, CV submission, or employment inquiry
- Consider phrases like: 'applying for', 'job application', 'resume', 'CV', 'position', 'employment', 'career opportunity'
- Return ONLY 'true' if this appears to be a job application or resume submission
- Return ONLY 'false' if this does not appear to be a job application

Your response must be exactly one word: either 'true' or 'false' (without quotes).");

            var isJobApplication = triageResult.ToString().Trim().ToLower();
            _logger.LogInformation($"Triage result: {isJobApplication}");

            if (isJobApplication != "true")
            {
                _logger.LogInformation("Email is not a job application, skipping processing");
                return "Email is not a job application";
            }

            // Step 3: Find PDF attachment and extract text
            var pdfAttachment = emailRequest.Attachments.FirstOrDefault(a => 
                a.ContentType.Contains("pdf", StringComparison.OrdinalIgnoreCase) ||
                a.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));

            if (pdfAttachment == null)
            {
                _logger.LogInformation("No PDF attachment found");
                return "No PDF resume found";
            }

            string resumeText;
            byte[] pdfBytes;

            try
            {
                pdfBytes = Convert.FromBase64String(pdfAttachment.ContentBytes);
                
                using var pdfDocument = PdfDocument.Open(pdfBytes);
                var textBuilder = new StringBuilder();
                
                foreach (var page in pdfDocument.GetPages())
                {
                    textBuilder.AppendLine(page.Text);
                }
                
                resumeText = textBuilder.ToString();
                _logger.LogInformation($"Extracted {resumeText.Length} characters from PDF");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from PDF");
                return "Error: Failed to extract text from PDF";
            }

            // Step 4: Analyze resume with AI
            _logger.LogInformation("Analyzing resume content with AI...");
            
            var analysisResult = await _kernel.InvokePromptAsync(
                $@"You are a professional resume screener with expertise in extracting key information from resumes. 

Analyze the following resume text and extract key information:

Resume Text:
{resumeText}

Instructions:
- Extract and structure the candidate's information
- Return the data as a single, minified JSON object
- Use the exact JSON schema below
- If information is not available, use empty string or empty array
- Ensure all dates are in ISO format (YYYY-MM-DD) if available
- For skills, extract both technical and soft skills
- For experience, focus on job titles, companies, and key responsibilities

Required JSON Schema:
{{
  ""name"": ""string"",
  ""email"": ""string"",
  ""phone"": ""string"",
  ""location"": ""string"",
  ""summary"": ""string"",
  ""skills"": [""string""],
  ""experience"": [
    {{
      ""jobTitle"": ""string"",
      ""company"": ""string"",
      ""startDate"": ""string"",
      ""endDate"": ""string"",
      ""description"": ""string""
    }}
  ],
  ""education"": [
    {{
      ""degree"": ""string"",
      ""institution"": ""string"",
      ""graduationDate"": ""string"",
      ""gpa"": ""string""
    }}
  ],
  ""certifications"": [""string""],
  ""languages"": [""string""]
}}

Return ONLY the JSON object with no additional text, explanations, or formatting.");

            var aiAnalysisText = analysisResult.ToString().Trim();
            _logger.LogInformation("AI analysis completed");

            // Clean up the AI response to ensure it's valid JSON
            var jsonStartIndex = aiAnalysisText.IndexOf('{');
            var jsonEndIndex = aiAnalysisText.LastIndexOf('}');
            
            if (jsonStartIndex >= 0 && jsonEndIndex > jsonStartIndex)
            {
                aiAnalysisText = aiAnalysisText.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);
            }

            // Step 5: Archive the resume
            var blobUrl = await StoreAttachmentAsync(pdfBytes, pdfAttachment.Name, emailRequest.InternetMessageId);
            _logger.LogInformation($"Resume archived to: {blobUrl}");

            // Step 6: Store analysis in Cosmos DB
            await StoreAnalysisInCosmosAsync(emailRequest, blobUrl, aiAnalysisText);
            _logger.LogInformation("Analysis stored in Cosmos DB");

            return "Resume processed successfully";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing resume");
            return $"Error: {ex.Message}";
        }
    }

    private async Task<string> StoreAttachmentAsync(byte[] fileBytes, string fileName, string messageId)
    {
        try
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient("resume-attachments");
            
            // Ensure container exists
            await containerClient.CreateIfNotExistsAsync();
            
            // Create unique blob name
            var blobName = $"{messageId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{fileName}";
            var blobClient = containerClient.GetBlobClient(blobName);
            
            // Upload the file
            using var stream = new MemoryStream(fileBytes);
            await blobClient.UploadAsync(stream, overwrite: true);
            
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing attachment");
            throw;
        }
    }

    private async Task StoreAnalysisInCosmosAsync(EmailRequest emailRequest, string blobUrl, string aiAnalysisJson)
    {
        try
        {
            var connectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString");
            var databaseName = Environment.GetEnvironmentVariable("CosmosDbDatabaseName");
            var containerName = Environment.GetEnvironmentVariable("CosmosDbContainerName");

            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName) || string.IsNullOrEmpty(containerName))
            {
                throw new InvalidOperationException("Cosmos DB configuration is missing");
            }

            var cosmosClient = new CosmosClient(connectionString);
            var container = cosmosClient.GetContainer(databaseName, containerName);

            // Parse AI analysis JSON
            object? aiAnalysisObject = null;
            try
            {
                aiAnalysisObject = JsonSerializer.Deserialize<object>(aiAnalysisJson);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse AI analysis as JSON, storing as string");
                aiAnalysisObject = aiAnalysisJson;
            }

            var resumeAnalysis = new ResumeAnalysis
            {
                InternetMessageId = emailRequest.InternetMessageId,
                EmailSubject = emailRequest.Subject,
                EmailFrom = emailRequest.From,
                EmailTo = emailRequest.To,
                ReceivedDateTime = emailRequest.ReceivedDateTime,
                ResumeBlobUrl = blobUrl,
                AiAnalysis = aiAnalysisObject,
                ProcessedDateTime = DateTime.UtcNow,
                Status = "Processed"
            };

            await container.CreateItemAsync(resumeAnalysis, new PartitionKey(resumeAnalysis.InternetMessageId));
            _logger.LogInformation($"Stored analysis document with ID: {resumeAnalysis.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing analysis in Cosmos DB");
            throw;
        }
    }
}
