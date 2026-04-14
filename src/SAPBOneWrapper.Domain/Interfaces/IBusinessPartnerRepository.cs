using SAPBOneWrapper.Domain.Entities;

namespace SAPBOneWrapper.Domain.Interfaces;

public interface IBusinessPartnerRepository
{
    Task<BusinessPartner?> GetByCardCodeAsync(string cardCode, CancellationToken ct = default);
    Task<(IReadOnlyList<BusinessPartner> Items, int TotalCount)> GetAllAsync(
        string? search, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(BusinessPartner businessPartner, CancellationToken ct = default);
    Task UpdateAsync(BusinessPartner businessPartner, CancellationToken ct = default);
    Task DeleteAsync(BusinessPartner businessPartner, CancellationToken ct = default);
    Task UpsertRangeAsync(IEnumerable<BusinessPartner> businessPartners, CancellationToken ct = default);
}
