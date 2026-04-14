using SAPBOneWrapper.Domain.Entities;

namespace SAPBOneWrapper.Domain.Interfaces;

public interface ISapB1ServiceLayerClient
{
    Task<BusinessPartner?> GetBusinessPartnerAsync(string cardCode, CancellationToken ct = default);
    Task<IReadOnlyList<BusinessPartner>> GetBusinessPartnersAsync(
        string? filter = null, int top = 20, int skip = 0, CancellationToken ct = default);
    Task<BusinessPartner> CreateBusinessPartnerAsync(BusinessPartner bp, CancellationToken ct = default);
    Task UpdateBusinessPartnerAsync(string cardCode, BusinessPartner bp, CancellationToken ct = default);
    Task DeleteBusinessPartnerAsync(string cardCode, CancellationToken ct = default);
}
