using FastEndpoints;
using MediatR;
using SAPBOneWrapper.Application.Features.BusinessPartners.Commands;

namespace SAPBOneWrapper.Api.Endpoints.BusinessPartners;

public class SyncResponse
{
    public int SyncedCount { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SyncBusinessPartnersEndpoint(IMediator mediator)
    : EndpointWithoutRequest<SyncResponse>
{
    public override void Configure()
    {
        Post("/business-partners/sync");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var count = await mediator.Send(new SyncBusinessPartnersCommand(), ct);

        await Send.OkAsync(new SyncResponse
        {
            SyncedCount = count,
            Message = $"Successfully synced {count} business partners from SAP B1."
        });
    }
}
