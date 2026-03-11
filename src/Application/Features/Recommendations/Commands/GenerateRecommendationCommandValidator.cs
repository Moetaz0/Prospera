using FluentValidation;

namespace Prospera.Application.Features.Recommendations.Commands;

/// <summary>
/// Validator for GenerateRecommendationCommand
/// </summary>
public class GenerateRecommendationCommandValidator : AbstractValidator<GenerateRecommendationCommand>
{
    public GenerateRecommendationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.AnalysisContext)
            .NotEmpty()
            .WithMessage("Analysis context is required")
            .MinimumLength(10)
            .WithMessage("Analysis context must be at least 10 characters")
            .MaximumLength(1000)
            .WithMessage("Analysis context cannot exceed 1000 characters");
    }
}
