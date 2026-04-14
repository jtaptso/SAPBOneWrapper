using MediatR;
using SAPBOneWrapper.Application.DTOs;

namespace SAPBOneWrapper.Application.Features.BusinessPartners.Queries;

public record GetBusinessPartnerByCodeQuery(string CardCode) : IRequest<BusinessPartnerDto?>;
