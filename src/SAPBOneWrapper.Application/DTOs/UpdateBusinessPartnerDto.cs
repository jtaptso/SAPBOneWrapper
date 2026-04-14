namespace SAPBOneWrapper.Application.DTOs;

public class UpdateBusinessPartnerDto
{
    public string CardName { get; set; } = string.Empty;
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
}
