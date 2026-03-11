using FluentValidation;

namespace Prospera.Application.Features.Transactions.Commands;

/// <summary>
/// Validator for AddTransactionCommand
/// </summary>
public class AddTransactionCommandValidator : AbstractValidator<AddTransactionCommand>
{
    public AddTransactionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Transaction amount must be greater than 0");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Transaction type is required")
            .Must(IsValidTransactionType)
            .WithMessage("Invalid transaction type. Must be: Income, Expense, Investment, or DebtPayment");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Transaction description is required")
            .MaximumLength(512)
            .WithMessage("Description cannot exceed 512 characters");
    }

    private static bool IsValidTransactionType(string type)
    {
        return type is "Income" or "Expense" or "Investment" or "DebtPayment";
    }
}
