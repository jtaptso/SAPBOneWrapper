using FastEndpoints;
using MediatR;
using SAPBOneWrapper.Application.DTOs;
using SAPBOneWrapper.Application.Features.BusinessPartners.Queries;

namespace SAPBOneWrapper.Api.Endpoints.BusinessPartners;

public class GetBusinessPartnerRequest
{
    public string CardCode { get; set; } = string.Empty;
}

public class GetBusinessPartnerEndpoint(IMediator mediator)
    : Endpoint<GetBusinessPartnerRequest, BusinessPartnerDto>
{
    public override void Configure()
    {
        Get("/business-partners/{CardCode}");
    }

    public override async Task HandleAsync(GetBusinessPartnerRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new GetBusinessPartnerByCodeQuery(req.CardCode), ct);

        if (result is null)
        {
            await Send.NotFoundAsync();
            return;
        }

        await Send.OkAsync(result);
    }
}
