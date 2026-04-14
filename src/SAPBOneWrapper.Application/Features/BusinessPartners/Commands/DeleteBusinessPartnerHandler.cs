using MediatR;
using SAPBOneWrapper.Domain.Interfaces;

namespace SAPBOneWrapper.Application.Features.BusinessPartners.Commands;

public class DeleteBusinessPartnerHandler(
    IBusinessPartnerRepository repository,
    ISapB1ServiceLayerClient sapClient)
    : IRequestHandler<DeleteBusinessPartnerCommand>
{
    public async Task Handle(DeleteBusinessPartnerCommand request, CancellationToken ct)
    {
        var bp = await repository.GetByCardCodeAsync(request.CardCode, ct)
            ?? throw new KeyNotFoundException($"Business Partner '{request.CardCode}' not found.");

        await sapClient.DeleteBusinessPartnerAsync(request.CardCode, ct);
        await repository.DeleteAsync(bp, ct);
    }
}
