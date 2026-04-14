using Mapster;
using MediatR;
using SAPBOneWrapper.Application.DTOs;
using SAPBOneWrapper.Domain.Enums;
using SAPBOneWrapper.Domain.Interfaces;

namespace SAPBOneWrapper.Application.Features.BusinessPartners.Commands;

public class UpdateBusinessPartnerHandler(
    IBusinessPartnerRepository repository,
    ISapB1ServiceLayerClient sapClient)
    : IRequestHandler<UpdateBusinessPartnerCommand, BusinessPartnerDto>
{
    public async Task<BusinessPartnerDto> Handle(UpdateBusinessPartnerCommand request, CancellationToken ct)
    {
        var bp = await repository.GetByCardCodeAsync(request.CardCode, ct)
            ?? throw new KeyNotFoundException($"Business Partner '{request.CardCode}' not found.");

        request.Dto.Adapt(bp);
        bp.SyncStatus = SyncStatus.PendingUpdate;
        bp.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(bp, ct);

        try
        {
            await sapClient.UpdateBusinessPartnerAsync(request.CardCode, bp, ct);
            bp.SyncStatus = SyncStatus.Synced;
            bp.LastSyncedAt = DateTime.UtcNow;
            await repository.UpdateAsync(bp, ct);
        }
        catch
        {
            bp.SyncStatus = SyncStatus.Error;
            await repository.UpdateAsync(bp, ct);
            throw;
        }

        return bp.Adapt<BusinessPartnerDto>();
    }
}
