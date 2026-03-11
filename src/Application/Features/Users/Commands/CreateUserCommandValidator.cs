using FluentValidation;

namespace Prospera.Application.Features.Users.Commands;

/// <summary>
/// Validator for CreateUserCommand
/// </summary>
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required")
            .MaximumLength(200)
            .WithMessage("Full name cannot exceed 200 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email format is invalid")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters");
    }
}
