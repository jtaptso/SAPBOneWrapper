using MediatR;
using SAPBOneWrapper.Application.DTOs;

namespace SAPBOneWrapper.Application.Features.BusinessPartners.Commands;

public record UpdateBusinessPartnerCommand(string CardCode, UpdateBusinessPartnerDto Dto) : IRequest<BusinessPartnerDto>;
