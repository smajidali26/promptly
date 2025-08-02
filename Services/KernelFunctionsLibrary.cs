using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace AgenticAI.Services;

/// <summary>
/// Library containing Semantic Kernel functions for AI-powered email and resume analysis
/// </summary>
public class KernelFunctionsLibrary
{
    /// <summary>
    /// Analyzes email content to determine if it represents a job application or resume submission
    /// </summary>
    /// <param name="emailSubject">The subject line of the email</param>
    /// <param name="emailBody">The body content of the email</param>
    /// <returns>AI prompt for email triage analysis</returns>
    [KernelFunction]
    [Description("Analyzes email subject and body to determine if it's a job application")]
    public string EmailBodyAnalysisFunction(
        [Description("The subject line of the email")] string emailSubject,
        [Description("The body content of the email")] string emailBody)
    {
        return $@"
You are an experienced HR assistant. Your task is to analyze an email and determine if it represents a job application or resume submission.

Email Subject: {emailSubject}
Email Body: {emailBody}

Instructions:
- Analyze the subject line and body content
- Look for keywords indicating job application, resume submission, CV submission, or employment inquiry
- Consider phrases like: 'applying for', 'job application', 'resume', 'CV', 'position', 'employment', 'career opportunity'
- Return ONLY 'true' if this appears to be a job application or resume submission
- Return ONLY 'false' if this does not appear to be a job application

Your response must be exactly one word: either 'true' or 'false' (without quotes).
";
    }

    /// <summary>
    /// Analyzes resume text to extract structured candidate information
    /// </summary>
    /// <param name="resumeText">The full text content extracted from the resume</param>
    /// <returns>AI prompt for resume analysis and data extraction</returns>
    [KernelFunction]
    [Description("Extracts structured information from resume text")]
    public string ResumeAnalysisFunction(
        [Description("The full text content extracted from the resume")] string resumeText)
    {
        return $@"
You are a professional resume screener with expertise in extracting key information from resumes. 

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

Return ONLY the JSON object with no additional text, explanations, or formatting.
";
    }
}
