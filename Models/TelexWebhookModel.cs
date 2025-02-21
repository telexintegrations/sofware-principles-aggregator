using System.Text.Json.Serialization;

namespace DevFactsAgregatorIntegration.Models;

public class TelexWebhookModel
{
    [JsonPropertyName("message")]
    public string Message { get; set; }
    [JsonPropertyName("username")]
    public string Username { get; set; }
    
    [JsonPropertyName("event_name")]
    public string EventName { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; }
}