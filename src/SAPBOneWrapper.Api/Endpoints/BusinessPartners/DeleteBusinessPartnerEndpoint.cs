using FastEndpoints;
using MediatR;
using SAPBOneWrapper.Application.Features.BusinessPartners.Commands;

namespace SAPBOneWrapper.Api.Endpoints.BusinessPartners;

public class DeleteBusinessPartnerRequest
{
    public string CardCode { get; set; } = string.Empty;
}

public class DeleteBusinessPartnerEndpoint(IMediator mediator)
    : Endpoint<DeleteBusinessPartnerRequest>
{
    public override void Configure()
    {
        Delete("/business-partners/{CardCode}");
    }

    public override async Task HandleAsync(DeleteBusinessPartnerRequest req, CancellationToken ct)
    {
        await mediator.Send(new DeleteBusinessPartnerCommand(req.CardCode), ct);
        await Send.NoContentAsync();
    }
}
