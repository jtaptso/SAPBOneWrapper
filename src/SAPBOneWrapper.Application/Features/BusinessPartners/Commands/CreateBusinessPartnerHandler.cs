using Mapster;
using MediatR;
using SAPBOneWrapper.Application.DTOs;
using SAPBOneWrapper.Domain.Entities;
using SAPBOneWrapper.Domain.Enums;
using SAPBOneWrapper.Domain.Interfaces;

namespace SAPBOneWrapper.Application.Features.BusinessPartners.Commands;

public class CreateBusinessPartnerHandler(
    IBusinessPartnerRepository repository,
    ISapB1ServiceLayerClient sapClient)
    : IRequestHandler<CreateBusinessPartnerCommand, BusinessPartnerDto>
{
    public async Task<BusinessPartnerDto> Handle(CreateBusinessPartnerCommand request, CancellationToken ct)
    {
        var bp = request.Dto.Adapt<BusinessPartner>();
        bp.SyncStatus = SyncStatus.PendingCreate;
        bp.CreatedAt = DateTime.UtcNow;
        bp.UpdatedAt = DateTime.UtcNow;

        await repository.AddAsync(bp, ct);

        try
        {
            await sapClient.CreateBusinessPartnerAsync(bp, ct);
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
