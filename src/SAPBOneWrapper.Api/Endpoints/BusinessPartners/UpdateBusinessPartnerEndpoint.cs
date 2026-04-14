using FastEndpoints;
using MediatR;
using SAPBOneWrapper.Application.DTOs;
using SAPBOneWrapper.Application.Features.BusinessPartners.Commands;

namespace SAPBOneWrapper.Api.Endpoints.BusinessPartners;

public class UpdateBusinessPartnerRequest
{
    public string CardCode { get; set; } = string.Empty;
    public string CardName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostCode { get; set; }
    public string? Currency { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? TaxId { get; set; }
    public bool Active { get; set; } = true;
    public string? Remarks { get; set; }
}

public class UpdateBusinessPartnerEndpoint(IMediator mediator)
    : Endpoint<UpdateBusinessPartnerRequest, BusinessPartnerDto>
{
    public override void Configure()
    {
        Put("/business-partners/{CardCode}");
    }

    public override async Task HandleAsync(UpdateBusinessPartnerRequest req, CancellationToken ct)
    {
        var dto = new UpdateBusinessPartnerDto
        {
            CardName = req.CardName,
            Phone = req.Phone,
            Email = req.Email,
            Address = req.Address,
            City = req.City,
            Country = req.Country,
            PostCode = req.PostCode,
            Currency = req.Currency,
            CreditLimit = req.CreditLimit,
            TaxId = req.TaxId,
            Active = req.Active,
            Remarks = req.Remarks
        };

        var result = await mediator.Send(new UpdateBusinessPartnerCommand(req.CardCode, dto), ct);
        await Send.OkAsync(result);
    }
}
