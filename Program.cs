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
           deploymentName: "gpt-35-turbo",
           endpoint: "https://semantic-kernel-openai001.openai.azure.com/",
           apiKey: "GC2JBHUgIddJRcM8iyJegMItyJ8UI4DYMhu46V4rrBrtS6PQXEeHJQQJ99BGACYeBjFXJ3w3AAABACOGPUu1");

// Register Semantic Kernel plugins
kernelBuilder.Plugins.AddFromType<KernelFunctionsLibrary>();

builder.Build().Run();
