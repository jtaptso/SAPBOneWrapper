using System.Text.Json.Serialization;

namespace SAPBOneWrapper.Infrastructure.SapServiceLayer.Models;

public class SapLoginResponse
{
    [JsonPropertyName("SessionId")]
    public string SessionId { get; set; } = string.Empty;
}
