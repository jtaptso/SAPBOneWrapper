using MediatR;

namespace SAPBOneWrapper.Application.Features.BusinessPartners.Commands;

public record SyncBusinessPartnersCommand : IRequest<int>;
