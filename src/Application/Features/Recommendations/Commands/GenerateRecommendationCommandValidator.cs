using FluentValidation;

namespace Prospera.Application.Features.Recommendations.Commands;

/// <summary>
/// Validator for GenerateRecommendationCommand
/// </summary>
public class GenerateRecommendationCommandValidator : AbstractValidator<GenerateRecommendationCommand>
{
    // Supported models for Groq and HuggingFace
    private static readonly HashSet<string> SupportedModels = new(StringComparer.OrdinalIgnoreCase)
    {
        // Groq models
        "mixtral-8x7b-32768",
        "llama2-70b-4096",
        "llama2-70b-chat-hf",
        "gemma-7b-it",

        // HuggingFace models
        "mistralai/Mistral-7B-Instruct-v0.2",
        "meta-llama/Llama-2-7b-chat-hf",
        "meta-llama/Llama-2-70b-chat-hf",
        "google/flan-t5-large",
        "google/flan-t5-xl",
        "microsoft/phi-2",
        "openchat/openchat-3.5",
        "NousResearch/Nous-Hermes-2-Mixtral-8x7B-DPO",
        "NousResearch/Nous-Hermes-2-SOLAR-10.7B"
    };

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

        // RuleFor(x => x.ModelName)
        //     .Must(modelName => modelName == null || SupportedModels.Contains(modelName))
        //     .WithMessage($"Model '{{{nameof(GenerateRecommendationCommand.ModelName)}}}' is not supported. Supported models: {string.Join(", ", SupportedModels.Order())}")
        //     .When(x => !string.IsNullOrWhiteSpace(x.ModelName));
    }
}
