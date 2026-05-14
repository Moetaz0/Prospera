using MediatR;
using Prospera.Application.Common.Interfaces;
using Prospera.Application.DTOs;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Features.Assets.Queries;

/// <summary>
/// Handler for GetAssetValuationQuery
/// Calculates asset valuations including projections for 1 year ahead
/// Uses automatic location-based rates with World Bank inflation data
/// For cars, integrates with Python financial-api for detailed depreciation schedules
/// Also provides personalized financial coaching for each asset
/// </summary>
public class GetAssetValuationQueryHandler : IRequestHandler<GetAssetValuationQuery, AssetValuationDto>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAssetValuationService _valuationService;
    private readonly IExternalRatesService _externalRatesService;
    private readonly ICoachingService _coachingService;
    private readonly ICarValuationApiService _carValuationApiService;

    public GetAssetValuationQueryHandler(
        IAssetRepository assetRepository,
        IUserRepository userRepository,
        IAssetValuationService valuationService,
        IExternalRatesService externalRatesService,
        ICoachingService coachingService,
        ICarValuationApiService carValuationApiService)
    {
        _assetRepository = assetRepository;
        _userRepository = userRepository;
        _valuationService = valuationService;
        _externalRatesService = externalRatesService;
        _coachingService = coachingService;
        _carValuationApiService = carValuationApiService;
    }

    public async Task<AssetValuationDto> Handle(GetAssetValuationQuery request, CancellationToken cancellationToken)
    {
        // Fetch asset from repository
        var asset = await _assetRepository.GetByIdAsync(request.AssetId, request.UserId);

        if (asset == null)
        {
            throw new KeyNotFoundException($"Asset with ID {request.AssetId} not found");
        }

        // Track where rates came from for transparency
        var rateSourceInfo = new RateSourceInfoDto
        {
            RatesFetchedAt = DateTime.UtcNow
        };

        // Get user's country for location-based rates
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user != null)
        {
            rateSourceInfo.UserCountry = user.Country;
        }

        // Determine rates to use - intelligently fetch from API if not provided
        decimal? inflationRate = request.InflationRate;
        decimal? carDepreciationRate = request.CarDepreciationRate;
        decimal? realEstateAppreciationRate = request.RealEstateAppreciationRate;

        // Track sources for transparency
        string inflationSource = "Default";
        string carDepreciationSource = "Default";
        string realEstateSource = "Default";

        // If no manual overrides provided, fetch from external rates API
        if (inflationRate == null || carDepreciationRate == null || realEstateAppreciationRate == null)
        {
            if (user != null && !string.IsNullOrWhiteSpace(user.Country))
            {
                try
                {
                    // Fetch inflation from World Bank (with caching and admin overrides)
                    if (inflationRate == null)
                    {
                        inflationRate = await _externalRatesService.GetInflationRateAsync(user.Country);
                        inflationSource = "World Bank API"; // or "Admin Override" if overridden
                    }
                    else
                    {
                        inflationSource = "Query Parameter (User Override)";
                    }

                    // Fetch car depreciation (regional defaults or admin overrides)
                    if (carDepreciationRate == null)
                    {
                        carDepreciationRate = await _externalRatesService.GetCarDepreciationRate(user.Country);
                        carDepreciationSource = "Regional Default"; // or "Admin Override"
                    }
                    else
                    {
                        carDepreciationSource = "Query Parameter (User Override)";
                    }

                    // Fetch real estate appreciation (regional defaults or admin overrides)
                    if (realEstateAppreciationRate == null)
                    {
                        realEstateAppreciationRate = await _externalRatesService.GetRealEstateAppreciationRate(user.Country);
                        realEstateSource = "Regional Default"; // or "Admin Override"
                    }
                    else
                    {
                        realEstateSource = "Query Parameter (User Override)";
                    }
                }
                catch (Exception)
                {
                    // If fetch fails, will fall back to defaults in CalculateProjectedValuation
                    inflationSource = "Default (API Fetch Failed)";
                    carDepreciationSource = "Default (API Fetch Failed)";
                    realEstateSource = "Default (API Fetch Failed)";
                }
            }
        }
        else
        {
            // All rates provided by user
            inflationSource = "Query Parameter (User Override)";
            carDepreciationSource = "Query Parameter (User Override)";
            realEstateSource = "Query Parameter (User Override)";
        }

        // Calculate valuation with determined rates
        var assetTypeString = asset.Type switch
        {
            Prospera.Domain.Enums.AssetType.Cash => "Cash",
            Prospera.Domain.Enums.AssetType.Stock => "Stock",
            Prospera.Domain.Enums.AssetType.Bond => "Bond",
            Prospera.Domain.Enums.AssetType.RealEstate => "RealEstate",
            Prospera.Domain.Enums.AssetType.Crypto => "Crypto",
            Prospera.Domain.Enums.AssetType.Car => "Car",
            Prospera.Domain.Enums.AssetType.Other => "Other",
            _ => asset.Type.ToString()
        };

        // For car assets, attempt to get detailed depreciation from Python financial-api
        if (asset.Type == Prospera.Domain.Enums.AssetType.Car && user != null)
        {
            try
            {
                // Try to get car valuation from Python financial-api
                // Default to assuming car was purchased 3 years ago if we don't have exact date
                var purchaseYear = DateTime.UtcNow.Year - 3;

                var carValuation = await _carValuationApiService.GetCarValuationAsync(
                    purchasePrice: asset.CurrentValue,
                    purchaseYear: purchaseYear,
                    targetYear: DateTime.UtcNow.Year + 1,
                    category: "sedan", // Default category
                    country: user.Country ?? "US");

                if (carValuation != null)
                {
                    carDepreciationRate = (decimal)carValuation.TotalDepreciationPercent / 100m;
                    carDepreciationSource = "Python Financial-API";
                }
            }
            catch
            {
                // Fall back to regional defaults if API call fails
            }
        }

        var valuation = _valuationService.CalculateProjectedValuation(
            asset.Id,
            asset.Name,
            assetTypeString,
            asset.CurrentValue,
            inflationRate,
            carDepreciationRate,
            realEstateAppreciationRate);

        // Add rate source information for transparency
        rateSourceInfo.InflationRate = inflationRate ?? 3.5m;
        rateSourceInfo.InflationRateSource = inflationSource;
        rateSourceInfo.CarDepreciationRate = carDepreciationRate ?? 15m;
        rateSourceInfo.CarDepreciationRateSource = carDepreciationSource;
        rateSourceInfo.RealEstateAppreciationRate = realEstateAppreciationRate ?? 3m;
        rateSourceInfo.RealEstateAppreciationRateSource = realEstateSource;
        rateSourceInfo.Summary = $"Inflation: {inflationSource} ({rateSourceInfo.InflationRate}%) | " +
                                 $"Car Depreciation: {carDepreciationSource} ({rateSourceInfo.CarDepreciationRate}%) | " +
                                 $"Real Estate Appreciation: {realEstateSource} ({rateSourceInfo.RealEstateAppreciationRate}%)";

        valuation.RateSourceInfo = rateSourceInfo;

        // Generate coaching advice for this asset
        try
        {
            var riskTolerance = CalculateRiskTolerance(user);
            var projectedValue = DetermineBestProjection(valuation);
            var portfolioAllocationPercentage = CalculateAllocationPercentage(asset.CurrentValue, user);

            valuation.CoachingAdvice = await _coachingService.GenerateAssetCoachingAdviceAsync(
                asset.Id,
                asset.Name,
                assetTypeString,
                asset.CurrentValue,
                projectedValue,
                valuation.ProjectionSummary,
                riskTolerance,
                portfolioAllocationPercentage,
                cancellationToken);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Warning: Failed to generate coaching advice: {ex.Message}");
            // Continue without coaching advice if generation fails
        }

        return valuation;
    }

    /// <summary>
    /// Calculate risk tolerance based on user profile
    /// </summary>
    private decimal CalculateRiskTolerance(Prospera.Domain.Entities.User? user)
    {
        if (user == null)
            return 0.5m;

        decimal riskScore = 0.5m;
        
        // This can be enhanced with actual user risk profile from database
        // For now, return moderate risk tolerance
        return Math.Max(0m, Math.Min(riskScore, 1m));
    }

    /// <summary>
    /// Determine the best projected value based on asset type
    /// </summary>
    private decimal DetermineBestProjection(AssetValuationDto valuation)
    {
        if (valuation.ProjectedValueRealEstate.HasValue)
            return valuation.ProjectedValueRealEstate.Value;
        
        if (valuation.ProjectedValueCar.HasValue)
            return valuation.ProjectedValueCar.Value;
        
        return valuation.ProjectedValueAfterInflation;
    }

    /// <summary>
    /// Calculate what percentage this asset is of total portfolio
    /// </summary>
    private decimal CalculateAllocationPercentage(decimal assetValue, Prospera.Domain.Entities.User? user)
    {
        // This can be enhanced to calculate based on actual portfolio
        // For now, return a placeholder
        return 0m;
    }
}

