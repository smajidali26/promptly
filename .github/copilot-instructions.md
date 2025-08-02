<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# CV Processing AI Agent Instructions

This is an Azure Function project that implements an AI-powered CV (resume) processing system using Microsoft Semantic Kernel and Azure OpenAI.

## Project Overview

The system processes incoming resume emails by:
1. Triggering on blob uploads (email JSON files)
2. Using AI to triage emails and determine if they contain job applications
3. Extracting text from PDF resume attachments
4. Analyzing resume content with AI to extract structured data
5. Archiving original resumes to Azure Blob Storage
6. Storing structured analysis results in Azure Cosmos DB

## Key Technologies

- **Azure Functions**: Serverless compute with blob triggers
- **Microsoft Semantic Kernel**: AI orchestration framework
- **Azure OpenAI**: GPT-4o for content analysis and extraction
- **Azure Blob Storage**: File storage for email triggers and resume archives
- **Azure Cosmos DB**: NoSQL database for structured resume data
- **PdfPig**: PDF text extraction library

## Architecture

- **Models**: Data transfer objects for emails and resume analysis
- **Services**: Semantic Kernel function library with AI prompts
- **Functions**: Main blob-triggered Azure Function for processing
- **Configuration**: Environment variables for Azure service connections

## Development Guidelines

When working with this codebase:
- Follow async/await patterns for all Azure SDK operations
- Use structured logging with appropriate log levels
- Handle exceptions gracefully with meaningful error messages
- Ensure all configuration values are externalized
- Follow the partition key pattern for Cosmos DB operations
- Use dependency injection for service registration
- Maintain separation of concerns between AI analysis and data storage
