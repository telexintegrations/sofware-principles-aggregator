using System.Text.Json.Serialization;

namespace DevFactsAgregatorIntegration.Models;

public class TelexSettingsModel
{
    [JsonPropertyName("label")]
    public string Label { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("required")]
    public bool Required { get; set; }
    
    [JsonPropertyName("default")]
    public string Default { get; set; }
}