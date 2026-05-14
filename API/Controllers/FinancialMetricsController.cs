using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prospera.Application.Features.FinancialMetrics.Queries;
using Prospera.Application.Features.Financial.Queries;
using Prospera.Contracts.DTOs.FinancialMetrics;

namespace Prospera.API.Controllers;

/// <summary>
/// Endpoints for retrieving and analyzing financial metrics
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class FinancialMetricsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FinancialMetricsController> _logger;

    public FinancialMetricsController(IMediator mediator, ILogger<FinancialMetricsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Calculate financial metrics for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Calculated financial metrics including savings rate, liquidity ratio, and debt ratio</returns>
    /// <response code="200">Metrics calculated successfully</response>
    /// <response code="404">User not found</response>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(FinancialMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFinancialMetrics(Guid userId)
    {
        _logger.LogInformation("Calculating financial metrics for user: {UserId}", userId);

        try
        {
            var query = new GetFinancialMetricsQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating financial metrics for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get savings rate for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Savings rate as percentage</returns>
    /// <response code="200">Savings rate retrieved</response>
    /// <response code="404">User not found</response>
    [HttpGet("users/{userId}/savings-rate")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSavingsRate(Guid userId)
    {
        _logger.LogInformation("Calculating savings rate for user: {UserId}", userId);

        try
        {
            var query = new GetSavingsRateQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating savings rate for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get liquidity ratio for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Liquidity ratio (liquid assets / total liabilities)</returns>
    /// <response code="200">Liquidity ratio retrieved</response>
    /// <response code="404">User not found</response>
    [HttpGet("users/{userId}/liquidity-ratio")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLiquidityRatio(Guid userId)
    {
        _logger.LogInformation("Calculating liquidity ratio for user: {UserId}", userId);

        try
        {
            var query = new GetLiquidityRatioQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating liquidity ratio for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get debt ratio for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Debt ratio (total liabilities / total assets)</returns>
    /// <response code="200">Debt ratio retrieved</response>
    /// <response code="404">User not found</response>
    [HttpGet("users/{userId}/debt-ratio")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDebtRatio(Guid userId)
    {
        _logger.LogInformation("Calculating debt ratio for user: {UserId}", userId);

        try
        {
            var query = new GetDebtRatioQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating debt ratio for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get inflation predictions for a country
    /// Uses Python financial-api for advanced predictions
    /// </summary>
    /// <param name="countryCode">ISO country code (e.g., "US", "FR", "TN")</param>
    /// <param name="yearsAhead">Number of years to predict (default: 5)</param>
    /// <returns>Inflation predictions for the specified years</returns>
    /// <response code="200">Predictions retrieved successfully</response>
    /// <response code="400">Invalid country code</response>
    [HttpGet("inflation/{countryCode}/predictions")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInflationPredictions(string countryCode, [FromQuery] int yearsAhead = 5)
    {
        _logger.LogInformation("Getting inflation predictions for country: {CountryCode}, years: {YearsAhead}",
            countryCode, yearsAhead);

        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
        {
            return BadRequest(new { error = "Invalid country code. Use ISO 3166-1 alpha-2 format (e.g., 'US', 'FR')" });
        }

        if (yearsAhead < 1 || yearsAhead > 20)
        {
            return BadRequest(new { error = "Years ahead must be between 1 and 20" });
        }

        try
        {
            var query = new GetInflationPredictionQuery
            {
                CountryCode = countryCode.ToUpper(),
                YearsAhead = yearsAhead
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inflation predictions for {CountryCode}", countryCode);
            throw;
        }
    }

    /// <summary>
    /// Get current inflation rate for a country
    /// </summary>
    /// <param name="countryCode">ISO country code (e.g., "US", "FR", "TN")</param>
    /// <returns>Current inflation rate</returns>
    /// <response code="200">Inflation rate retrieved</response>
    /// <response code="400">Invalid country code</response>
    [HttpGet("inflation/{countryCode}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInflationRate(string countryCode)
    {
        _logger.LogInformation("Getting current inflation rate for country: {CountryCode}", countryCode);

        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
        {
            return BadRequest(new { error = "Invalid country code. Use ISO 3166-1 alpha-2 format (e.g., 'US', 'FR')" });
        }

        try
        {
            // Would need to inject IExternalRatesService here
            // For now, return a placeholder response
            return Ok(new { countryCode = countryCode.ToUpper(), message = "Use /api/FinancialMetrics/inflation/{countryCode}/predictions for detailed predictions" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inflation rate for {CountryCode}", countryCode);
            throw;
        }
    }
}
