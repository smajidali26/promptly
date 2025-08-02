using AgenticAI.Config;
using AgenticAI.Services;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = FunctionsApplication.CreateBuilder(args);

        // Enable Functions environment (telemetry, DI, etc.)
        builder.ConfigureFunctionsWebApplication();

        // Add Application Insights (optional)
        builder.Services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights();

        var config = builder.Configuration;

        // Register configuration sections
        builder.Services.Configure<AzureOpenAIConfig>(config.GetSection("AzureOpenAI"));
        builder.Services.Configure<BlobStorageConfig>(config.GetSection("BlobStorage"));
        builder.Services.Configure<CosmosDbConfig>(config.GetSection("CosmosDb"));

        // Load AzureOpenAI config directly (throwing exception if missing)
        var azureOpenAIConfig = config.GetSection("AzureOpenAI").Get<AzureOpenAIConfig>()
            ?? throw new InvalidOperationException("Missing AzureOpenAI configuration");

        // Register Semantic Kernel
        var kernelBuilder = builder.Services.AddKernel();

        kernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName: azureOpenAIConfig.DeploymentName,
            endpoint: azureOpenAIConfig.Endpoint,
            apiKey: azureOpenAIConfig.ApiKey
        );

        // Register plugins
        kernelBuilder.Plugins.AddFromType<KernelFunctionsLibrary>();

        builder.Build().Run();
    }
}
