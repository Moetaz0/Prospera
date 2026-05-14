using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prospera.Application.Common.Interfaces;
using Prospera.Domain.Entities;
using Prospera.Domain.Interfaces;

namespace Prospera.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that fetches financial news daily from NewsAPI
/// Runs automatically every 24 hours
/// </summary>
public class DailyNewsBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailyNewsBackgroundService> _logger;
    private readonly TimeSpan _executionTime;
    private Timer? _timer;

    public DailyNewsBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DailyNewsBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        // Set to run at 8:00 AM every day
        _executionTime = new TimeSpan(8, 0, 0);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily News Background Service starting...");

        // Calculate time until next execution
        var now = DateTime.Now;
        var targetTime = now.Date.Add(_executionTime);

        // If the target time has already passed today, schedule for tomorrow
        if (targetTime <= now)
        {
            targetTime = targetTime.AddDays(1);
        }

        var initialDelay = targetTime - now;

        _logger.LogInformation("Next news fetch scheduled for: {TargetTime} (in {Delay})", targetTime, initialDelay);

        // Initial timer to run at the scheduled time
        _timer = new Timer(
            async _ => await FetchAndCacheNewsAsync(stoppingToken),
            null,
            initialDelay,
            TimeSpan.FromHours(24)); // Repeat every 24 hours

        // Also run once on startup (optional - set to TimeSpan.Zero to skip)
        await FetchAndCacheNewsAsync(stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task FetchAndCacheNewsAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting daily news fetch at {Timestamp}", DateTime.UtcNow);

            using var scope = _serviceProvider.CreateAsyncScope();
            var newsService = scope.ServiceProvider.GetRequiredService<IFinancialNewsService>();
            var newsRepository = scope.ServiceProvider.GetRequiredService<INewsRepository>();

            // Fetch different types of news
            var businessNews = await newsService.GetLatestNewsAsync(pageSize: 20, cancellationToken);
            var cryptoNews = await newsService.GetCryptoNewsAsync(pageSize: 15, cancellationToken);
            var trendingNews = await newsService.GetTrendingNewsAsync(pageSize: 15, cancellationToken);

            var articlesToCache = new List<CachedNewsArticle>();

            // Process business news
            if (businessNews?.Success == true && businessNews.Articles.Any())
            {
                var businessArticles = businessNews.Articles
                    .Select(a => new CachedNewsArticle
                    {
                        Title = a.Title,
                        Description = a.Description,
                        Url = a.Url,
                        Image = a.Image,
                        Source = a.Source,
                        Category = a.Category,
                        PublishedAt = a.PublishedAt,
                        Sentiment = a.Sentiment,
                        NewsType = "business",
                        CachedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList();

                articlesToCache.AddRange(businessArticles);
                _logger.LogInformation("Fetched {Count} business news articles", businessArticles.Count);
            }

            // Process crypto news
            if (cryptoNews?.Success == true && cryptoNews.Articles.Any())
            {
                var cryptoArticles = cryptoNews.Articles
                    .Select(a => new CachedNewsArticle
                    {
                        Title = a.Title,
                        Description = a.Description,
                        Url = a.Url,
                        Image = a.Image,
                        Source = a.Source,
                        Category = a.Category,
                        PublishedAt = a.PublishedAt,
                        Sentiment = a.Sentiment,
                        NewsType = "crypto",
                        CachedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList();

                articlesToCache.AddRange(cryptoArticles);
                _logger.LogInformation("Fetched {Count} crypto news articles", cryptoArticles.Count);
            }

            // Process trending news
            if (trendingNews?.Success == true && trendingNews.Articles.Any())
            {
                var trendingArticles = trendingNews.Articles
                    .Select(a => new CachedNewsArticle
                    {
                        Title = a.Title,
                        Description = a.Description,
                        Url = a.Url,
                        Image = a.Image,
                        Source = a.Source,
                        Category = a.Category,
                        PublishedAt = a.PublishedAt,
                        Sentiment = a.Sentiment,
                        NewsType = "trending",
                        CachedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList();

                articlesToCache.AddRange(trendingArticles);
                _logger.LogInformation("Fetched {Count} trending news articles", trendingArticles.Count);
            }

            // Clear old news and cache new ones
            if (articlesToCache.Any())
            {
                // Remove old articles (older than 7 days)
                await newsRepository.DeleteOldAsync(olderThanDays: 7, cancellationToken);

                // Add new articles
                await newsRepository.AddRangeAsync(articlesToCache, cancellationToken);

                _logger.LogInformation(
                    "Daily news fetch completed successfully. Cached {Count} articles total",
                    articlesToCache.Count);
            }
            else
            {
                _logger.LogWarning("No news articles were fetched");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during daily news fetch");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Daily News Background Service stopping...");
        _timer?.Dispose();
        await base.StopAsync(cancellationToken);
    }

    /// <summary>
    /// Public method to manually trigger the news fetch (useful for testing/API endpoints)
    /// </summary>
    public async Task TriggerNewsRefreshAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Manual news refresh triggered");
        await FetchAndCacheNewsAsync(cancellationToken);
    }
}
