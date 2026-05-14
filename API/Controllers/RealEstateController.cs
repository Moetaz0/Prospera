using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prospera.Application.Common.Interfaces;
using Prospera.Domain.Interfaces;

namespace Prospera.API.Controllers;

/// <summary>
/// Real estate property valuation endpoints
/// Calculate and track appreciation of residential/commercial properties
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RealEstateController : ControllerBase
{
    private readonly IRealEstateEvaluationRepository _evaluationRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly IFinancialDataService _financialDataService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RealEstateController> _logger;

    public RealEstateController(
        IRealEstateEvaluationRepository evaluationRepository,
        IAssetRepository assetRepository,
        IFinancialDataService financialDataService,
        ICurrentUserService currentUserService,
        ILogger<RealEstateController> logger)
    {
        _evaluationRepository = evaluationRepository;
        _assetRepository = assetRepository;
        _financialDataService = financialDataService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Calculate and save a real estate evaluation
    /// Fetches market data from Financial API and computes appreciation forecasts
    /// </summary>
    [HttpPost("evaluate")]
    [Authorize]
    public async Task<ActionResult<object>> EvaluateProperty([FromBody] EvaluatePropertyRequest request)
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

            // Get real estate market data
            var realEstateData = request.CountryCode.ToUpper() == "TN"
                ? await _financialDataService.GetTunisianRealEstateAsync(request.Location, CancellationToken.None)
                : await _financialDataService.GetGlobalRealEstateAsync(request.CountryCode, CancellationToken.None);

            if (realEstateData == null || realEstateData.Data == null || realEstateData.Data.Count == 0)
                return BadRequest($"No real estate data available for {request.Location}");

            // Calculate appreciation forecasts
            var latestData = realEstateData.Data.LastOrDefault();
            var avgGrowthRate = realEstateData.AverageAnnualGrowth;
            var yearsHeld = DateTime.UtcNow.Year - request.PurchaseYear;

            // Compute future values (5-year, 10-year)
            var forecast5Year = request.PurchasePrice * (decimal)Math.Pow((double)(1 + avgGrowthRate / 100), 5);
            var forecast10Year = request.PurchasePrice * (decimal)Math.Pow((double)(1 + avgGrowthRate / 100), 10);

            // Current estimated value based on years held
            var currentValue = request.PurchasePrice * (decimal)Math.Pow((double)(1 + avgGrowthRate / 100), yearsHeld);
            var totalAppreciation = ((currentValue - request.PurchasePrice) / request.PurchasePrice) * 100;

            // Create and save evaluation
            var evaluation = new Prospera.Domain.Entities.RealEstateEvaluation(
                userId,
                request.AssetId,
                request.Location,
                request.CountryCode.ToUpper(),
                request.PurchasePrice,
                request.PurchaseYear,
                currentValue,
                avgGrowthRate,
                totalAppreciation,
                forecast5Year,
                forecast10Year,
                "Financial API");

            await _evaluationRepository.AddAsync(evaluation);

            return Ok(new
            {
                assetId = request.AssetId,
                location = request.Location,
                countryCode = request.CountryCode.ToUpper(),
                purchasePrice = request.PurchasePrice,
                purchaseYear = request.PurchaseYear,
                currentEstimatedValue = Math.Round(currentValue, 2),
                annualGrowthRate = Math.Round(avgGrowthRate, 2),
                totalAppreciationPct = Math.Round(totalAppreciation, 2),
                forecast5YearValue = Math.Round(forecast5Year, 2),
                forecast10YearValue = Math.Round(forecast10Year, 2),
                evaluatedAt = evaluation.EvaluatedAt,
                message = "Property evaluation saved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating property");
            return StatusCode(500, new { message = "Error evaluating property", error = ex.Message });
        }
    }

    /// <summary>
    /// Get evaluation history for a property asset
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
                currentEstimatedValue = e.CurrentEstimatedValue,
                annualGrowthRate = e.AnnualGrowthRate,
                totalAppreciationPct = e.TotalAppreciationPct,
                forecast5YearValue = e.Forecast5YearValue,
                forecast10YearValue = e.Forecast10YearValue,
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
    /// Get the most recent evaluation for a property asset
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
                location = evaluation.Location,
                countryCode = evaluation.CountryCode,
                purchasePrice = evaluation.PurchasePrice,
                purchaseYear = evaluation.PurchaseYear,
                currentEstimatedValue = evaluation.CurrentEstimatedValue,
                annualGrowthRate = evaluation.AnnualGrowthRate,
                totalAppreciationPct = evaluation.TotalAppreciationPct,
                forecast5YearValue = evaluation.Forecast5YearValue,
                forecast10YearValue = evaluation.Forecast10YearValue,
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
    /// Get available real estate market locations (Tunisia)
    /// </summary>
    [HttpGet("locations")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetLocations()
    {
        try
        {
            var locations = await _financialDataService.GetRealEstateLocationsAsync(CancellationToken.None);
            return Ok(new { locations });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving locations");
            return StatusCode(500, new { message = "Error retrieving locations", error = ex.Message });
        }
    }

    /// <summary>
    /// Get market data for a Tunisia location
    /// </summary>
    [HttpGet("market/{location}")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetMarketData(string location)
    {
        try
        {
            var data = await _financialDataService.GetTunisianRealEstateAsync(location, CancellationToken.None);
            if (data?.Data == null || data.Data.Count == 0)
                return NotFound($"No market data found for {location}");

            return Ok(new
            {
                location = data.Location,
                country = data.Country,
                averageAnnualGrowth = data.AverageAnnualGrowth,
                historicalData = data.Data.Select(d => new
                {
                    d.Year,
                    d.GrowthRate,
                    d.IndexValue
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving market data");
            return StatusCode(500, new { message = "Error retrieving market data", error = ex.Message });
        }
    }
}

/// <summary>
/// Request model for evaluating a property
/// </summary>
public class EvaluatePropertyRequest
{
    public Guid AssetId { get; set; }
    public string Location { get; set; } = string.Empty; // e.g., "Tunis", "Sfax"
    public string CountryCode { get; set; } = string.Empty; // "TN", "FR", "US"
    public decimal PurchasePrice { get; set; }
    public int PurchaseYear { get; set; }
}
