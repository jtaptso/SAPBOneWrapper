using FastEndpoints;
using MediatR;
using SAPBOneWrapper.Application.DTOs;
using SAPBOneWrapper.Application.Features.BusinessPartners.Queries;

namespace SAPBOneWrapper.Api.Endpoints.BusinessPartners;

public class ListBusinessPartnersRequest
{
    [QueryParam]
    public string? Search { get; set; }

    [QueryParam]
    public int Page { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 20;
}

public class ListBusinessPartnersEndpoint(IMediator mediator)
    : Endpoint<ListBusinessPartnersRequest, PagedResultDto<BusinessPartnerDto>>
{
    public override void Configure()
    {
        Get("/business-partners");
    }

    public override async Task HandleAsync(ListBusinessPartnersRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetBusinessPartnersQuery(req.Search, req.Page, req.PageSize), ct);
        await Send.OkAsync(result);
    }
}
