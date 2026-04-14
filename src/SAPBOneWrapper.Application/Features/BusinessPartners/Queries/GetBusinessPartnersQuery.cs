using MediatR;
using SAPBOneWrapper.Application.DTOs;

namespace SAPBOneWrapper.Application.Features.BusinessPartners.Queries;

public record GetBusinessPartnersQuery(string? Search, int Page = 1, int PageSize = 20)
    : IRequest<PagedResultDto<BusinessPartnerDto>>;
