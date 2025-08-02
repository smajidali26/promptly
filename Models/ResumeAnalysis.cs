using System.Text.Json.Serialization;

namespace AgenticAI.Models;

public class ResumeAnalysis
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("internetMessageId")]
    public string InternetMessageId { get; set; } = string.Empty;

    [JsonPropertyName("emailSubject")]
    public string EmailSubject { get; set; } = string.Empty;

    [JsonPropertyName("emailFrom")]
    public string EmailFrom { get; set; } = string.Empty;

    [JsonPropertyName("emailTo")]
    public string EmailTo { get; set; } = string.Empty;

    [JsonPropertyName("receivedDateTime")]
    public DateTime ReceivedDateTime { get; set; }

    [JsonPropertyName("resumeBlobUrl")]
    public string ResumeBlobUrl { get; set; } = string.Empty;

    [JsonPropertyName("aiAnalysis")]
    public object? AiAnalysis { get; set; }

    [JsonPropertyName("processedDateTime")]
    public DateTime ProcessedDateTime { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Processed";
}
