using MediatR;
using SAPBOneWrapper.Application.DTOs;

namespace SAPBOneWrapper.Application.Features.BusinessPartners.Commands;

public record CreateBusinessPartnerCommand(CreateBusinessPartnerDto Dto) : IRequest<BusinessPartnerDto>;
