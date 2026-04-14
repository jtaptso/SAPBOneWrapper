using Microsoft.EntityFrameworkCore;
using SAPBOneWrapper.Domain.Entities;
using SAPBOneWrapper.Domain.Enums;
using SAPBOneWrapper.Domain.Interfaces;
using SAPBOneWrapper.Infrastructure.Data;

namespace SAPBOneWrapper.Infrastructure.Repositories;

public class BusinessPartnerRepository(AppDbContext context) : IBusinessPartnerRepository
{
    public async Task<BusinessPartner?> GetByCardCodeAsync(string cardCode, CancellationToken ct = default)
    {
        return await context.BusinessPartners.FindAsync([cardCode], ct);
    }

    public async Task<(IReadOnlyList<BusinessPartner> Items, int TotalCount)> GetAllAsync(
        string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.BusinessPartners.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(bp =>
                bp.CardCode.ToLower().Contains(term) ||
                bp.CardName.ToLower().Contains(term) ||
                (bp.Email != null && bp.Email.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(bp => bp.CardName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(BusinessPartner businessPartner, CancellationToken ct = default)
    {
        context.BusinessPartners.Add(businessPartner);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(BusinessPartner businessPartner, CancellationToken ct = default)
    {
        context.BusinessPartners.Update(businessPartner);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(BusinessPartner businessPartner, CancellationToken ct = default)
    {
        context.BusinessPartners.Remove(businessPartner);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpsertRangeAsync(IEnumerable<BusinessPartner> businessPartners, CancellationToken ct = default)
    {
        foreach (var bp in businessPartners)
        {
            var existing = await context.BusinessPartners.FindAsync([bp.CardCode], ct);
            if (existing is null)
            {
                bp.CreatedAt = DateTime.UtcNow;
                bp.UpdatedAt = DateTime.UtcNow;
                context.BusinessPartners.Add(bp);
            }
            else
            {
                existing.CardName = bp.CardName;
                existing.CardType = bp.CardType;
                existing.Phone = bp.Phone;
                existing.Email = bp.Email;
                existing.Address = bp.Address;
                existing.City = bp.City;
                existing.Country = bp.Country;
                existing.PostCode = bp.PostCode;
                existing.Currency = bp.Currency;
                existing.CreditLimit = bp.CreditLimit;
                existing.TaxId = bp.TaxId;
                existing.Active = bp.Active;
                existing.Remarks = bp.Remarks;
                existing.SyncStatus = bp.SyncStatus;
                existing.LastSyncedAt = bp.LastSyncedAt;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync(ct);
    }
}
