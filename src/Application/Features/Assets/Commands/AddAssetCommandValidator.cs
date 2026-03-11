using FluentValidation;

namespace Prospera.Application.Features.Assets.Commands;

/// <summary>
/// Validator for AddAssetCommand
/// </summary>
public class AddAssetCommandValidator : AbstractValidator<AddAssetCommand>
{
    public AddAssetCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Asset name is required")
            .MaximumLength(256)
            .WithMessage("Asset name cannot exceed 256 characters");

        RuleFor(x => x.CurrentValue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Asset value must be greater than or equal to 0");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Asset type is required")
            .Must(IsValidAssetType)
            .WithMessage("Invalid asset type. Must be: Cash, Stock, Bond, RealEstate, Crypto, or Other");
    }

    private static bool IsValidAssetType(string type)
    {
        return type is "Cash" or "Stock" or "Bond" or "RealEstate" or "Crypto" or "Other";
    }
}
