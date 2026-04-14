using System.Text.Json.Serialization;

namespace SAPBOneWrapper.Infrastructure.SapServiceLayer.Models;

public class SapBusinessPartnerModel
{
    [JsonPropertyName("CardCode")]
    public string CardCode { get; set; } = string.Empty;

    [JsonPropertyName("CardName")]
    public string? CardName { get; set; }

    [JsonPropertyName("CardType")]
    public string? CardType { get; set; }

    [JsonPropertyName("Phone1")]
    public string? Phone { get; set; }

    [JsonPropertyName("EmailAddress")]
    public string? Email { get; set; }

    [JsonPropertyName("Address")]
    public string? Address { get; set; }

    [JsonPropertyName("City")]
    public string? City { get; set; }

    [JsonPropertyName("Country")]
    public string? Country { get; set; }

    [JsonPropertyName("ZipCode")]
    public string? PostCode { get; set; }

    [JsonPropertyName("Currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("MaxCommitment")]
    public decimal? CreditLimit { get; set; }

    [JsonPropertyName("FederalTaxID")]
    public string? TaxId { get; set; }

    [JsonPropertyName("Valid")]
    public string? Active { get; set; }

    [JsonPropertyName("FreeText")]
    public string? Remarks { get; set; }
}
