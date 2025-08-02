using System.Text.Json.Serialization;

namespace AgenticAI.Models;

public class EmailRequest
{
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("attachments")]
    public List<EmailAttachment> Attachments { get; set; } = new();

    [JsonPropertyName("internetMessageId")]
    public string InternetMessageId { get; set; } = string.Empty;

    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("to")]
    public string To { get; set; } = string.Empty;

    [JsonPropertyName("receivedDateTime")]
    public DateTime ReceivedDateTime { get; set; }
}

public class EmailAttachment
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("contentType")]
    public string ContentType { get; set; } = string.Empty;

    [JsonPropertyName("contentBytes")]
    public string ContentBytes { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public int Size { get; set; }
}
