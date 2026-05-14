using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prospera.Application.Common.Interfaces;
using Prospera.Domain.Interfaces;

namespace Prospera.API.Controllers;

/// <summary>
/// Car valuation and depreciation endpoints
/// Calculate and track depreciation of vehicles
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CarsController : ControllerBase
{
    private readonly ICarEvaluationRepository _evaluationRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly IFinancialDataService _financialDataService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CarsController> _logger;

    public CarsController(
        ICarEvaluationRepository evaluationRepository,
        IAssetRepository assetRepository,
        IFinancialDataService financialDataService,
        ICurrentUserService currentUserService,
        ILogger<CarsController> logger)
    {
        _evaluationRepository = evaluationRepository;
        _assetRepository = assetRepository;
        _financialDataService = financialDataService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Calculate and save a car depreciation evaluation
    /// Fetches depreciation schedule from Financial API based on car details
    /// </summary>
    [HttpPost("evaluate")]
    [Authorize]
    public async Task<ActionResult<object>> EvaluateVehicle([FromBody] EvaluateCarRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
                return Unauthorized("User ID not found");

            // Verify asset belongs to user
            var asset = await _assetRepository.GetByIdAsync(request.AssetId, userId);
            if (asset == null)
                return NotFound($"Asset {request.AssetId} not found for current user");

            // Get car depreciation schedule from Financial API
            var depreciationData = await _financialDataService.GetCarDepreciationAsync(
                request.PurchasePrice,
                DateTime.UtcNow.Year - request.PurchaseYear,
                request.CountryCode,
                CancellationToken.None);

            if (depreciationData == null || depreciationData.Schedule == null || depreciationData.Schedule.Count == 0)
                return BadRequest("Unable to calculate car depreciation");

            // Extract current market value and total depreciation from schedule
            var currentYearDepreciation = depreciationData.Schedule.Last();
            var currentValue = currentYearDepreciation.Value;
            var totalDepreciation = ((request.PurchasePrice - currentValue) / request.PurchasePrice) * 100;
            var inflationAdjusted = currentValue; // use current value as proxy

            // Calculate 5-year forecast
            var yearsAhead = 5;
            var forecast5Year = currentValue;
            var currentYearIndex = DateTime.UtcNow.Year - request.PurchaseYear;
            var targetYearIndex = currentYearIndex + yearsAhead;
            if (targetYearIndex > 0 && targetYearIndex <= depreciationData.Schedule.Count)
            {
                forecast5Year = depreciationData.Schedule[targetYearIndex - 1].Value;
            }

            // Get single-year depreciation rate (first year)
            var annualDepreciationRate = depreciationData.Schedule.Count > 0
                ? depreciationData.Schedule[0].DepreciationPercentage
                : 15m; // fallback default

            // Create and save evaluation
            var evaluation = new Prospera.Domain.Entities.CarEvaluation(
                userId,
                request.AssetId,
                request.Make,
                request.Model,
                request.Category,
                request.CountryCode.ToUpper(),
                request.PurchasePrice,
                request.PurchaseYear,
                request.AnnualMileageKm,
                currentValue,
                totalDepreciation,
                annualDepreciationRate,
                inflationAdjusted,
                forecast5Year,
                "Financial API");

            await _evaluationRepository.AddAsync(evaluation);

            // Return depreciation schedule details
            var yearlyBreakdown = depreciationData.Schedule.Select(y => new
            {
                y.Year,
                y.Value,
                depreciation_rate_pct = y.DepreciationPercentage,
                depreciation_amount = y.DepreciationAmount
            });

            return Ok(new
            {
                assetId = request.AssetId,
                make = request.Make,
                model = request.Model,
                category = request.Category,
                countryCode = request.CountryCode.ToUpper(),
                purchasePrice = request.PurchasePrice,
                purchaseYear = request.PurchaseYear,
                annualMileageKm = request.AnnualMileageKm,
                currentMarketValue = Math.Round(currentValue, 2),
                totalDepreciationPct = Math.Round(totalDepreciation, 2),
                inflationAdjustedValue = Math.Round(inflationAdjusted, 2),
                forecast5YearValue = Math.Round(forecast5Year, 2),
                yearlyBreakdown,
                evaluatedAt = DateTime.UtcNow,
                message = "Vehicle evaluation saved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating vehicle");
            return StatusCode(500, new { message = "Error evaluating vehicle", error = ex.Message });
        }
    }

    /// <summary>
    /// Get evaluation history for a car asset
    /// Returns all past evaluations in reverse chronological order
    /// </summary>
    [HttpGet("{assetId}/history")]
    [Authorize]
    public async Task<ActionResult<object>> GetHistory(Guid assetId)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
                return Unauthorized("User ID not found");

            // Verify asset belongs to user
            var asset = await _assetRepository.GetByIdAsync(assetId, userId);
            if (asset == null)
                return NotFound($"Asset {assetId} not found");

            var evaluations = await _evaluationRepository.GetByAssetIdAsync(assetId, userId);
            if (!evaluations.Any())
                return Ok(new { assetId, evaluations = new object[0], message = "No evaluations found" });

            var result = evaluations.Select(e => new
            {
                evaluationId = e.Id,
                currentMarketValue = e.CurrentMarketValue,
                totalDepreciationPct = e.TotalDepreciationPct,
                annualDepreciationRate = e.AnnualDepreciationRate,
                inflationAdjustedValue = e.InflationAdjustedValue,
                forecast5YearValue = e.Forecast5YearValue,
                evaluatedAt = e.EvaluatedAt
            });

            return Ok(new { assetId, evaluations = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving evaluation history");
            return StatusCode(500, new { message = "Error retrieving history", error = ex.Message });
        }
    }

    /// <summary>
    /// Get the most recent evaluation for a car asset
    /// </summary>
    [HttpGet("{assetId}/latest")]
    [Authorize]
    public async Task<ActionResult<object>> GetLatest(Guid assetId)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
                return Unauthorized("User ID not found");

            var evaluation = await _evaluationRepository.GetLatestByAssetIdAsync(assetId, userId);
            if (evaluation == null)
                return NotFound($"No evaluation found for asset {assetId}");

            return Ok(new
            {
                evaluationId = evaluation.Id,
                assetId = evaluation.AssetId,
                make = evaluation.Make,
                model = evaluation.Model,
                category = evaluation.Category,
                countryCode = evaluation.CountryCode,
                purchasePrice = evaluation.PurchasePrice,
                purchaseYear = evaluation.PurchaseYear,
                annualMileageKm = evaluation.AnnualMileageKm,
                currentMarketValue = evaluation.CurrentMarketValue,
                totalDepreciationPct = evaluation.TotalDepreciationPct,
                annualDepreciationRate = evaluation.AnnualDepreciationRate,
                inflationAdjustedValue = evaluation.InflationAdjustedValue,
                forecast5YearValue = evaluation.Forecast5YearValue,
                evaluatedAt = evaluation.EvaluatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving latest evaluation");
            return StatusCode(500, new { message = "Error retrieving evaluation", error = ex.Message });
        }
    }

    /// <summary>
    /// Get available car categories and their depreciation multipliers
    /// </summary>
    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetCategories()
    {
        try
        {
            var categories = await _financialDataService.GetCarCategoriesAsync(CancellationToken.None);
            if (categories == null || !categories.Any())
                return NotFound("No car categories found");

            var result = categories.Select(c => new
            {
                c.Name,
                c.DepreciationMultiplier,
                c.Description
            });

            return Ok(new { categories = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving car categories");
            return StatusCode(500, new { message = "Error retrieving categories", error = ex.Message });
        }
    }
}

/// <summary>
/// Request model for evaluating a car
/// </summary>
public class EvaluateCarRequest
{
    public Guid AssetId { get; set; }
    public string Make { get; set; } = string.Empty; // e.g., "Toyota"
    public string Model { get; set; } = string.Empty; // e.g., "Corolla"
    public string Category { get; set; } = string.Empty; // "sedan", "suv", "truck"
    public string CountryCode { get; set; } = string.Empty; // "TN", "FR", "US"
    public decimal PurchasePrice { get; set; }
    public int PurchaseYear { get; set; }
    public int AnnualMileageKm { get; set; }
}
