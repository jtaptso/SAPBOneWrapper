using FluentValidation;
using SAPBOneWrapper.Application.Features.BusinessPartners.Commands;

namespace SAPBOneWrapper.Application.Features.BusinessPartners.Validators;

public class UpdateBusinessPartnerValidator : AbstractValidator<UpdateBusinessPartnerCommand>
{
    public UpdateBusinessPartnerValidator()
    {
        RuleFor(x => x.CardCode)
            .NotEmpty().WithMessage("Card Code is required.");

        RuleFor(x => x.Dto.CardName)
            .NotEmpty().WithMessage("Card Name is required.")
            .MaximumLength(100).WithMessage("Card Name must not exceed 100 characters.");

        RuleFor(x => x.Dto.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Dto.Email))
            .WithMessage("A valid email address is required.");
    }
}
