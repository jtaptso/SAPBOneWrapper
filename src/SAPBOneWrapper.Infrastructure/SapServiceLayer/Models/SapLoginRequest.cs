using System.Text.Json.Serialization;

namespace SAPBOneWrapper.Infrastructure.SapServiceLayer.Models;

public class SapLoginRequest
{
    [JsonPropertyName("CompanyDB")]
    public string CompanyDB { get; set; } = string.Empty;

    [JsonPropertyName("UserName")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("Password")]
    public string Password { get; set; } = string.Empty;
}
