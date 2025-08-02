namespace AgenticAI.Config
{
    public class CosmosDbConfig
    {
        public string AccountEndpoint { get; set; } = string.Empty;
        public string AccountKey { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
    }
}
