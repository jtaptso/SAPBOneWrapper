using SAPBOneWrapper.Domain.Enums;

namespace SAPBOneWrapper.Application.DTOs;

public class BusinessPartnerDto
{
    public string CardCode { get; set; } = string.Empty;
    public string CardName { get; set; } = string.Empty;
    public CardType CardType { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostCode { get; set; }
    public string? Currency { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? TaxId { get; set; }
    public bool Active { get; set; }
    public string? Remarks { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}
