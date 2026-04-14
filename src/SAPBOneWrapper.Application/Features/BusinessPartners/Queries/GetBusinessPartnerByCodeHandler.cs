using Mapster;
using MediatR;
using SAPBOneWrapper.Application.DTOs;
using SAPBOneWrapper.Domain.Interfaces;

namespace SAPBOneWrapper.Application.Features.BusinessPartners.Queries;

public class GetBusinessPartnerByCodeHandler(IBusinessPartnerRepository repository)
    : IRequestHandler<GetBusinessPartnerByCodeQuery, BusinessPartnerDto?>
{
    public async Task<BusinessPartnerDto?> Handle(GetBusinessPartnerByCodeQuery request, CancellationToken ct)
    {
        var bp = await repository.GetByCardCodeAsync(request.CardCode, ct);
        return bp?.Adapt<BusinessPartnerDto>();
    }
}
