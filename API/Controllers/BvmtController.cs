using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prospera.Application.Common.Interfaces;

namespace Prospera.API.Controllers;

/// <summary>
/// BVMT (Tunisian Stock Exchange) market data endpoints
/// Provides access to BVMT stock listings, analysis, and halal ratings
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BvmtController : ControllerBase
{
    private readonly IBvmtService _bvmtService;
    private readonly ILogger<BvmtController> _logger;

    public BvmtController(IBvmtService bvmtService, ILogger<BvmtController> logger)
    {
        _bvmtService = bvmtService;
        _logger = logger;
    }

    /// <summary>
    /// Get all BVMT stocks with optional halal filtering
    /// Public endpoint for browsing available securities
    /// </summary>
    /// <param name="halalOnly">Filter to show only halal-compliant stocks</param>
    [HttpGet("stocks")]
    [AllowAnonymous]
    public async Task<ActionResult<BvmtStockListData>> GetAllStocks([FromQuery] bool halalOnly = false)
    {
        try
        {
            var stocks = await _bvmtService.GetAllStocksAsync(halalOnly);
            if (stocks?.Stocks == null || stocks.Stocks.Count == 0)
            {
                return NotFound("No BVMT stocks found");
            }

            return Ok(stocks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching BVMT stocks");
            return StatusCode(500, new { message = "Error fetching BVMT stocks", error = ex.Message });
        }
    }

    /// <summary>
    /// Get a single BVMT stock by ticker
    /// Public endpoint for viewing stock details
    /// </summary>
    /// <param name="ticker">BVMT stock ticker (e.g., "ATB", "BH", "UIB")</param>
    [HttpGet("stocks/{ticker}")]
    [AllowAnonymous]
    public async Task<ActionResult<BvmtStockData>> GetStockByTicker(string ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker))
        {
            return BadRequest("Ticker is required");
        }

        try
        {
            var stock = await _bvmtService.GetStockByTickerAsync(ticker.ToUpper());
            if (stock == null)
            {
                return NotFound($"BVMT stock '{ticker}' not found");
            }

            return Ok(stock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching BVMT stock {ticker}");
            return StatusCode(500, new { message = $"Error fetching stock {ticker}", error = ex.Message });
        }
    }

    /// <summary>
    /// Get AI-powered stock analysis with recommendation
    /// Includes SWOT analysis, price target, and investment recommendation
    /// Requires authentication - uses LLM analysis credits
    /// </summary>
    /// <param name="ticker">BVMT stock ticker</param>
    [HttpGet("stocks/{ticker}/analysis")]
    [Authorize] // Requires login - analysis uses LLM credits
    public async Task<ActionResult<BvmtStockAnalysisData>> GetStockAnalysis(string ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker))
        {
            return BadRequest("Ticker is required");
        }

        try
        {
            var analysis = await _bvmtService.GetStockAnalysisAsync(ticker.ToUpper());
            if (analysis == null)
            {
                return NotFound($"Analysis not available for BVMT stock '{ticker}'");
            }

            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching BVMT stock analysis for {ticker}");
            return StatusCode(500, new { message = $"Error fetching analysis for {ticker}", error = ex.Message });
        }
    }

    /// <summary>
    /// Get all BVMT sectors with aggregated statistics
    /// Public endpoint for market overview
    /// </summary>
    [HttpGet("sectors")]
    [AllowAnonymous]
    public async Task<ActionResult<BvmtSectorsData>> GetSectors()
    {
        try
        {
            var sectors = await _bvmtService.GetSectorsAsync();
            if (sectors?.Data == null || sectors.Data.Count == 0)
            {
                return NotFound("No BVMT sector data found");
            }

            return Ok(sectors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching BVMT sectors");
            return StatusCode(500, new { message = "Error fetching BVMT sectors", error = ex.Message });
        }
    }

    /// <summary>
    /// Get only halal-compliant BVMT stocks
    /// Public endpoint for Islamic finance screening
    /// </summary>
    [HttpGet("halal-stocks")]
    [AllowAnonymous]
    public async Task<ActionResult<BvmtStockListData>> GetHalalStocks()
    {
        try
        {
            var stocks = await _bvmtService.GetHalalStocksAsync();
            if (stocks?.Stocks == null || stocks.Stocks.Count == 0)
            {
                return NotFound("No halal BVMT stocks found");
            }

            return Ok(stocks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching halal BVMT stocks");
            return StatusCode(500, new { message = "Error fetching halal stocks", error = ex.Message });
        }
    }

    /// <summary>
    /// Get only haram (non-compliant) BVMT stocks
    /// Public endpoint for complete market view
    /// </summary>
    [HttpGet("haram-stocks")]
    [AllowAnonymous]
    public async Task<ActionResult<BvmtStockListData>> GetHaramStocks()
    {
        try
        {
            var stocks = await _bvmtService.GetHaramStocksAsync();
            if (stocks?.Stocks == null || stocks.Stocks.Count == 0)
            {
                return NotFound("No haram BVMT stocks found");
            }

            return Ok(stocks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching haram BVMT stocks");
            return StatusCode(500, new { message = "Error fetching haram stocks", error = ex.Message });
        }
    }
}
