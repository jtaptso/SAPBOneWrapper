using Mapster;
using MediatR;
using SAPBOneWrapper.Application.DTOs;
using SAPBOneWrapper.Domain.Interfaces;

namespace SAPBOneWrapper.Application.Features.BusinessPartners.Queries;

public class GetBusinessPartnersHandler(IBusinessPartnerRepository repository)
    : IRequestHandler<GetBusinessPartnersQuery, PagedResultDto<BusinessPartnerDto>>
{
    public async Task<PagedResultDto<BusinessPartnerDto>> Handle(GetBusinessPartnersQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await repository.GetAllAsync(request.Search, request.Page, request.PageSize, ct);

        return new PagedResultDto<BusinessPartnerDto>
        {
            Items = items.Adapt<IReadOnlyList<BusinessPartnerDto>>(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
