using MediatR;
using SAPBOneWrapper.Domain.Enums;
using SAPBOneWrapper.Domain.Interfaces;

namespace SAPBOneWrapper.Application.Features.BusinessPartners.Commands;

public class SyncBusinessPartnersHandler(
    IBusinessPartnerRepository repository,
    ISapB1ServiceLayerClient sapClient)
    : IRequestHandler<SyncBusinessPartnersCommand, int>
{
    public async Task<int> Handle(SyncBusinessPartnersCommand request, CancellationToken ct)
    {
        var sapPartners = await sapClient.GetBusinessPartnersAsync(ct: ct);

        foreach (var bp in sapPartners)
        {
            bp.SyncStatus = SyncStatus.Synced;
            bp.LastSyncedAt = DateTime.UtcNow;
        }

        await repository.UpsertRangeAsync(sapPartners, ct);

        return sapPartners.Count;
    }
}
