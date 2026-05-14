using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prospera.Application.Common.Interfaces;

namespace Prospera.API.Controllers;

/// <summary>
/// Admin endpoints for managing financial rates
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RatesController : ControllerBase
{
    private readonly IExternalRatesService _ratesService;

    public RatesController(IExternalRatesService ratesService)
    {
        _ratesService = ratesService;
    }

    /// <summary>
    /// Get all current rates with source information
    /// Shows which rates are from World Bank, admin override, or hardcoded defaults
    /// </summary>
    [HttpGet("current")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CurrentRates>>> GetCurrentRates()
    {
        var rates = await _ratesService.GetAllCurrentRatesAsync();
        return Ok(rates);
    }

    /// <summary>
    /// Set custom rates for a country (admin override)
    /// Can override one or more rates; null values keep existing rates
    /// </summary>
    /// <param name="countryCode">ISO country code (e.g., "TN", "US")</param>
    /// <param name="request">Custom rates to set</param>
    [HttpPost("{countryCode}/custom")]
    public async Task<ActionResult> SetCustomRates(string countryCode, [FromBody] SetCustomRatesRequest request)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return BadRequest("Country code is required");
        }

        if (countryCode.Length != 2)
        {
            return BadRequest("Country code must be ISO 3166-1 alpha-2 format (e.g., 'US', 'DE')");
        }

        // Validate rate values if provided
        if (request.InflationRate.HasValue && (request.InflationRate < 0 || request.InflationRate > 100))
        {
            return BadRequest("Inflation rate must be between 0 and 100");
        }

        if (request.CarDepreciationRate.HasValue && (request.CarDepreciationRate < 0 || request.CarDepreciationRate > 100))
        {
            return BadRequest("Car depreciation rate must be between 0 and 100");
        }

        if (request.RealEstateAppreciationRate.HasValue && (request.RealEstateAppreciationRate < -50 || request.RealEstateAppreciationRate > 100))
        {
            return BadRequest("Real estate appreciation rate must be between -50 and 100");
        }

        try
        {
            await _ratesService.SetCustomRatesAsync(
                countryCode.ToUpper(),
                request.InflationRate,
                request.CarDepreciationRate,
                request.RealEstateAppreciationRate);

            return Ok(new { message = "Custom rates set successfully", countryCode = countryCode.ToUpper() });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error setting custom rates", error = ex.Message });
        }
    }

    /// <summary>
    /// Clear custom rates for a country and revert to defaults
    /// </summary>
    /// <param name="countryCode">ISO country code</param>
    [HttpDelete("{countryCode}/custom")]
    public async Task<ActionResult> ClearCustomRates(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return BadRequest("Country code is required");
        }

        try
        {
            // Check if custom rates exist
            var hasCustom = await _ratesService.HasCustomRatesAsync(countryCode.ToUpper());
            if (!hasCustom)
            {
                return NotFound($"No custom rates found for country {countryCode}");
            }

            await _ratesService.ClearCustomRatesAsync(countryCode.ToUpper());
            return Ok(new { message = "Custom rates cleared. Reverted to defaults.", countryCode = countryCode.ToUpper() });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error clearing custom rates", error = ex.Message });
        }
    }

    /// <summary>
    /// Get custom rates for a specific country (if set)
    /// </summary>
    /// <param name="countryCode">ISO country code</param>
    [HttpGet("{countryCode}/custom")]
    public async Task<ActionResult<CustomRates>> GetCustomRates(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return BadRequest("Country code is required");
        }

        var customRates = await _ratesService.GetCustomRatesAsync(countryCode.ToUpper());
        
        if (customRates == null)
        {
            return NotFound($"No custom rates set for country {countryCode}. System using defaults.");
        }

        return Ok(customRates);
    }

    /// <summary>
    /// Manually trigger a refresh of inflation rates from World Bank
    /// Useful for immediate updates without waiting for background job
    /// </summary>
    /// <param name="countryCodeFilter">Optional: refresh only specific country</param>
    [HttpPost("refresh-inflation")]
    public async Task<ActionResult> RefreshInflationRates([FromQuery] string? countryCodeFilter = null)
    {
        try
        {
            await _ratesService.RefreshInflationRatesAsync(countryCodeFilter?.ToUpper());
            
            var message = countryCodeFilter != null
                ? $"Inflation rates refreshed for {countryCodeFilter.ToUpper()}"
                : "All inflation rates refreshed from World Bank API";

            return Ok(new { message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error refreshing rates", error = ex.Message });
        }
    }
}

/// <summary>
/// Request model for setting custom rates
/// </summary>
public class SetCustomRatesRequest
{
    /// <summary>
    /// Annual inflation rate (%) - leave null to keep existing
    /// </summary>
    public decimal? InflationRate { get; set; }

    /// <summary>
    /// Annual car depreciation rate (%) - leave null to keep existing
    /// </summary>
    public decimal? CarDepreciationRate { get; set; }

    /// <summary>
    /// Annual real estate appreciation rate (%) - leave null to keep existing
    /// </summary>
    public decimal? RealEstateAppreciationRate { get; set; }

    /// <summary>
    /// Reason for the override (e.g., "War in region", "Economic crisis")
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// When this override should expire (optional)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
