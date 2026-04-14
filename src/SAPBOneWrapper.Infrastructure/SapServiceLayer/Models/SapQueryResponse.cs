using System.Text.Json.Serialization;

namespace SAPBOneWrapper.Infrastructure.SapServiceLayer.Models;

public class SapQueryResponse<T>
{
    [JsonPropertyName("value")]
    public List<T> Value { get; set; } = [];

    [JsonPropertyName("odata.nextLink")]
    public string? NextLink { get; set; }
}
