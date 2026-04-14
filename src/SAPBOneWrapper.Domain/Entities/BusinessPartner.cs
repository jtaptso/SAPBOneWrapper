using SAPBOneWrapper.Domain.Enums;

namespace SAPBOneWrapper.Domain.Entities;

public class BusinessPartner
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
    public bool Active { get; set; } = true;
    public string? Remarks { get; set; }
    public SyncStatus SyncStatus { get; set; } = SyncStatus.PendingCreate;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSyncedAt { get; set; }
}
