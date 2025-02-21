using System.Text.Json.Serialization;

namespace DevFactsAgregatorIntegration.Models;

public class TelexPayloadModel
{
    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; }
    
    [JsonPropertyName("return_url")]
    public string ReturnUrl { get; set; }
    
    [JsonPropertyName("settings")]
    public List<TelexSettingsModel> Settings { get; set; }
}