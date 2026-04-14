using FastEndpoints;
using MediatR;
using SAPBOneWrapper.Application.DTOs;
using SAPBOneWrapper.Application.Features.BusinessPartners.Commands;

namespace SAPBOneWrapper.Api.Endpoints.BusinessPartners;

public class CreateBusinessPartnerEndpoint(IMediator mediator)
    : Endpoint<CreateBusinessPartnerDto, BusinessPartnerDto>
{
    public override void Configure()
    {
        Post("/business-partners");
    }

    public override async Task HandleAsync(CreateBusinessPartnerDto req, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateBusinessPartnerCommand(req), ct);
        await Send.ResponseAsync(result, StatusCodes.Status201Created);
    }
}
