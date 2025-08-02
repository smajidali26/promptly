# AI Agent for CV Processing

An Azure Function-based AI agent that processes incoming resume emails using Microsoft Semantic Kernel and Azure OpenAI. The system intelligently analyzes email content, extracts information from PDF resumes, and stores structured data for further processing.

## Features

- **Intelligent Email Triage**: Uses AI to determine if emails contain job applications
- **PDF Text Extraction**: Extracts text content from PDF resume attachments
- **AI-Powered Analysis**: Leverages Azure OpenAI GPT-4o to extract structured information from resumes
- **Automatic Archiving**: Stores original resume files in Azure Blob Storage
- **Structured Data Storage**: Saves analysis results in Azure Cosmos DB for NoSQL querying

## Architecture

```
Email JSON → Blob Trigger → AI Triage → PDF Extraction → AI Analysis → Archive + Store
```

### Components

1. **Azure Function**: Blob-triggered serverless function
2. **Semantic Kernel**: AI orchestration and prompt management
3. **Azure OpenAI**: GPT-4o model for content analysis
4. **Azure Blob Storage**: Email triggers and resume archival
5. **Azure Cosmos DB**: Structured resume data storage
6. **PdfPig**: PDF text extraction

## Prerequisites

- Azure Subscription with appropriate permissions
- .NET 8 SDK
- Azure Functions Core Tools
- Visual Studio Code (recommended)

### Required Azure Resources

1. **Azure AI Foundry**: GPT-4o chat completion model
2. **Azure Cosmos DB**: NoSQL database with `/InternetMessageId` partition key
3. **Azure Storage Account**: Two blob containers:
   - `received-mails`: Incoming JSON trigger files
   - `resume-attachments`: Archived PDF resumes

## Setup

### 1. Clone and Install Dependencies

```bash
# Dependencies are already included in the project file
dotnet restore
```

### 2. Configure Local Settings

Update `local.settings.json` with your Azure resource connection strings:

```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "DefaultEndpointsProtocol=https;AccountName=...",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
        "CosmosDbConnectionString": "AccountEndpoint=https://...",
        "CosmosDbDatabaseName": "ResumeDatabase",
        "CosmosDbContainerName": "ResumeAnalyses",
        "AzureOpenAIChatCompletion:DeploymentName": "gpt-4o",
        "AzureOpenAIChatCompletion:Endpoint": "https://your-resource.openai.azure.com/"
    }
}
```

### 3. Azure Resource Configuration

#### Storage Account
- Create containers: `received-mails` and `resume-attachments`
- Configure blob access level as needed

#### Cosmos DB
- Create database: `ResumeDatabase`
- Create container: `ResumeAnalyses` with partition key `/InternetMessageId`

#### Azure OpenAI
- Deploy GPT-4o model
- Note the deployment name and endpoint

## Usage

### Running Locally

```bash
# Start the Azure Functions runtime
func start
```

### Testing

1. Place a test email JSON file in the `received-mails` blob container
2. Monitor function logs for processing status
3. Check `resume-attachments` container for archived files
4. Verify structured data in Cosmos DB container

### Sample Email JSON Format

```json
{
    "subject": "Job Application - Software Developer",
    "body": "Dear Hiring Manager, I am applying for the software developer position...",
    "internetMessageId": "unique-message-id",
    "from": "candidate@example.com",
    "to": "hr@company.com",
    "receivedDateTime": "2025-08-02T10:00:00Z",
    "attachments": [
        {
            "name": "resume.pdf",
            "contentType": "application/pdf",
            "contentBytes": "base64-encoded-pdf-content",
            "size": 1024000
        }
    ]
}
```

## Project Structure

```
AgenticAI/
├── Functions/
│   └── ResumeAnalyzerFunction.cs    # Main blob-triggered function
├── Models/
│   ├── EmailRequest.cs              # Email data models
│   └── ResumeAnalysis.cs           # Resume analysis data model
├── Services/
│   └── KernelFunctionsLibrary.cs   # Semantic Kernel AI prompts
├── Program.cs                       # Dependency injection configuration
├── local.settings.json             # Local configuration
└── AgenticAI.csproj                # Project dependencies
```

## Key Dependencies

- `Microsoft.Azure.Functions.Worker.Extensions.Storage.Blobs`: Blob triggers
- `Microsoft.SemanticKernel`: AI orchestration framework
- `Microsoft.Azure.Cosmos`: Cosmos DB integration
- `UglyToad.PdfPig`: PDF text extraction
- `Azure.Storage.Blobs`: Blob storage operations

## AI Processing Pipeline

1. **Email Triage**: AI determines if email contains job application
2. **PDF Processing**: Extract text from resume attachments
3. **Structured Analysis**: AI extracts candidate information into JSON format
4. **Data Storage**: Archive original file and store structured data

## Deployment

### Azure Function App

1. Create Azure Function App with .NET 8 runtime
2. Configure application settings with connection strings
3. Deploy using VS Code Azure Functions extension or Azure CLI

### Environment Variables

Set the following in your Azure Function App configuration:

- `AzureWebJobsStorage`
- `CosmosDbConnectionString`
- `CosmosDbDatabaseName`
- `CosmosDbContainerName`
- `AzureOpenAIChatCompletion:DeploymentName`
- `AzureOpenAIChatCompletion:Endpoint`

## Monitoring

- View function execution logs in Azure Portal
- Monitor Cosmos DB operations and storage
- Track blob storage usage and operations
- Use Application Insights for detailed telemetry

## Security Considerations

- Use Azure Managed Identity for production deployments
- Secure connection strings as Key Vault references
- Implement appropriate RBAC for Azure resources
- Consider data privacy regulations for resume processing

## Troubleshooting

### Common Issues

1. **Authentication Errors**: Verify Azure credentials and permissions
2. **Blob Trigger Not Firing**: Check storage account connection string
3. **Cosmos DB Errors**: Verify database and container names
4. **PDF Processing Errors**: Ensure PDF files are not corrupted or encrypted

### Debugging

Enable detailed logging in `host.json`:

```json
{
    "version": "2.0",
    "logging": {
        "applicationInsights": {
            "samplingSettings": {
                "isEnabled": true,
                "excludedTypes": "Request"
            }
        },
        "logLevel": {
            "default": "Information"
        }
    }
}
```

## Contributing

1. Follow the existing code structure and patterns
2. Add appropriate error handling and logging
3. Update documentation for new features
4. Test with sample data before deployment

## License

This project is licensed under the MIT License.
