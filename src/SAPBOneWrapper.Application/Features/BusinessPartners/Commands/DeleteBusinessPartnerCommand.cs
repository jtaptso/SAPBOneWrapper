using MediatR;

namespace SAPBOneWrapper.Application.Features.BusinessPartners.Commands;

public record DeleteBusinessPartnerCommand(string CardCode) : IRequest;
