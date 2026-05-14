using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prospera.Application.Features.News.Queries;
using Prospera.Infrastructure.BackgroundServices;

namespace Prospera.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class NewsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<NewsController> _logger;

    public NewsController(IMediator mediator, ILogger<NewsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets latest financial news from all sources
    /// </summary>
    /// <param name="pageSize">Number of articles to return (default: 10, max: 100)</param>
    /// <returns>Latest financial news articles</returns>
    [HttpGet("latest")]
    [ProducesResponseType(typeof(GetLatestNewsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLatestNews([FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("News/Latest endpoint called - PageSize: {PageSize}", pageSize);

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { error = "PageSize must be between 1 and 100" });
        }

        var query = new GetLatestNewsQuery { PageSize = pageSize };
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Gets financial news by category
    /// </summary>
    /// <param name="category">Category: stocks, crypto, banking, real estate, bonds, forex, commodities</param>
    /// <param name="pageSize">Number of articles to return (default: 10, max: 100)</param>
    /// <returns>News articles for the specified category</returns>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(GetNewsByCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetNewsByCategory(
        [FromRoute] string category,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("News/Category endpoint called - Category: {Category}, PageSize: {PageSize}", category, pageSize);

        if (string.IsNullOrWhiteSpace(category))
        {
            return BadRequest(new { error = "Category is required" });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { error = "PageSize must be between 1 and 100" });
        }

        var query = new GetNewsByCategoryQuery
        {
            Category = category,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Searches for financial news by keyword
    /// </summary>
    /// <param name="keyword">Search keyword (e.g., "Apple", "Bitcoin", "stock market")</param>
    /// <param name="pageSize">Number of articles to return (default: 10, max: 100)</param>
    /// <returns>News articles matching the search keyword</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(GetNewsByCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchNews(
        [FromQuery] string keyword,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("News/Search endpoint called - Keyword: {Keyword}, PageSize: {PageSize}", keyword, pageSize);

        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest(new { error = "Keyword is required" });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { error = "PageSize must be between 1 and 100" });
        }

        var query = new GetNewsByCategoryQuery
        {
            Category = keyword,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Gets latest cryptocurrency news
    /// </summary>
    /// <param name="pageSize">Number of articles to return (default: 10, max: 100)</param>
    /// <returns>Latest cryptocurrency news articles</returns>
    [HttpGet("crypto")]
    [ProducesResponseType(typeof(GetNewsByCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCryptoNews([FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("News/Crypto endpoint called - PageSize: {PageSize}", pageSize);

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { error = "PageSize must be between 1 and 100" });
        }

        var query = new GetNewsByCategoryQuery
        {
            Category = "crypto",
            PageSize = pageSize
        };
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Gets trending financial news
    /// </summary>
    /// <param name="pageSize">Number of articles to return (default: 10, max: 100)</param>
    /// <returns>Trending financial news articles</returns>
    [HttpGet("trending")]
    [ProducesResponseType(typeof(GetTrendingNewsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTrendingNews([FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("News/Trending endpoint called - PageSize: {PageSize}", pageSize);

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { error = "PageSize must be between 1 and 100" });
        }

        var query = new GetTrendingNewsQuery { PageSize = pageSize };
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Gets cached news articles (from daily scheduled fetch)
    /// This endpoint returns news that was cached by the background service
    /// </summary>
    /// <param name="newsType">News type: business, crypto, trending (optional - omit for all)</param>
    /// <param name="pageSize">Number of articles to return (default: 20, max: 100)</param>
    /// <returns>Cached financial news articles from database</returns>
    [HttpGet("cached")]
    [ProducesResponseType(typeof(GetCachedNewsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCachedNews([FromQuery] string? newsType = null, [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("News/Cached endpoint called - NewsType: {NewsType}, PageSize: {PageSize}", newsType ?? "all", pageSize);

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { error = "PageSize must be between 1 and 100" });
        }

        var query = new GetCachedNewsQuery
        {
            NewsType = newsType,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Manually trigger the daily news fetch (for testing/development)
    /// This endpoint is useful for testing without waiting for the scheduled 8:00 AM execution
    /// </summary>
    /// <returns>Result of the manual news fetch operation</returns>
    [HttpPost("refresh-cache")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshNewsCache()
    {
        _logger.LogInformation("Manual news cache refresh triggered via API");

        try
        {
            // Get the background service instance
            var hostedServices = HttpContext.RequestServices.GetServices<IHostedService>();
            var dailyNewsService = hostedServices
                .OfType<DailyNewsBackgroundService>()
                .FirstOrDefault();

            if (dailyNewsService == null)
            {
                _logger.LogError("DailyNewsBackgroundService not found in hosted services");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "Daily news service not available" });
            }

            // Trigger the public news refresh method
            await dailyNewsService.TriggerNewsRefreshAsync(CancellationToken.None);

            _logger.LogInformation("News cache refresh completed successfully");
            return Ok(new { message = "News cache refreshed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during manual news cache refresh");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to refresh news cache", details = ex.Message });
        }
    }
}
