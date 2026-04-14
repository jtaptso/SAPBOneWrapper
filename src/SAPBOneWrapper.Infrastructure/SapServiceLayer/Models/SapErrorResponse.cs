using System.Text.Json.Serialization;

namespace SAPBOneWrapper.Infrastructure.SapServiceLayer.Models;

public class SapErrorResponse
{
    [JsonPropertyName("error")]
    public SapError? Error { get; set; }
}

public class SapError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public SapErrorMessage? Message { get; set; }
}

public class SapErrorMessage
{
    [JsonPropertyName("lang")]
    public string? Lang { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
