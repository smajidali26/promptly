using AgenticAI.Services;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add Application Insights
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Add Semantic Kernel
var kernelBuilder = builder.Services.AddKernel();

kernelBuilder.AddAzureOpenAIChatCompletion(
    deploymentName: Environment.GetEnvironmentVariable("AzureOpenAIChatCompletion:DeploymentName") ?? throw new InvalidOperationException("Missing Azure OpenAI deployment name"),
    endpoint: Environment.GetEnvironmentVariable("AzureOpenAIChatCompletion:Endpoint") ?? throw new InvalidOperationException("Missing Azure OpenAI endpoint"),
    credentials: new DefaultAzureCredential());

// Register Semantic Kernel plugins
kernelBuilder.Plugins.AddFromType<KernelFunctionsLibrary>();

builder.Build().Run();
