using FluentValidation;
using SAPBOneWrapper.Application.Features.BusinessPartners.Commands;

namespace SAPBOneWrapper.Application.Features.BusinessPartners.Validators;

public class CreateBusinessPartnerValidator : AbstractValidator<CreateBusinessPartnerCommand>
{
    public CreateBusinessPartnerValidator()
    {
        RuleFor(x => x.Dto.CardCode)
            .NotEmpty().WithMessage("Card Code is required.")
            .Length(3, 15).WithMessage("Card Code must be between 3 and 15 characters.");

        RuleFor(x => x.Dto.CardName)
            .NotEmpty().WithMessage("Card Name is required.")
            .MaximumLength(100).WithMessage("Card Name must not exceed 100 characters.");

        RuleFor(x => x.Dto.CardType)
            .IsInEnum().WithMessage("Card Type must be Customer, Supplier, or Lead.");

        RuleFor(x => x.Dto.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Dto.Email))
            .WithMessage("A valid email address is required.");
    }
}
