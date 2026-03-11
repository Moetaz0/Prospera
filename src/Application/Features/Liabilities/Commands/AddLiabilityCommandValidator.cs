using FluentValidation;

namespace Prospera.Application.Features.Liabilities.Commands;

/// <summary>
/// Validator for AddLiabilityCommand
/// </summary>
public class AddLiabilityCommandValidator : AbstractValidator<AddLiabilityCommand>
{
    public AddLiabilityCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Liability name is required")
            .MaximumLength(256)
            .WithMessage("Liability name cannot exceed 256 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Liability amount must be greater than 0");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Liability type is required")
            .Must(IsValidLiabilityType)
            .WithMessage("Invalid liability type. Must be: CreditCard, Loan, Mortgage, LineOfCredit, or Other");
    }

    private static bool IsValidLiabilityType(string type)
    {
        return type is "CreditCard" or "Loan" or "Mortgage" or "LineOfCredit" or "Other";
    }
}
