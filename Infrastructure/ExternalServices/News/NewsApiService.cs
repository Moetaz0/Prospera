using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prospera.Application.Common.Interfaces;

namespace Prospera.Infrastructure.ExternalServices.News;

/// <summary>
/// NewsAPI.org implementation for fetching financial news
/// Free tier: 100 requests/day
/// </summary>
public class NewsApiService : IFinancialNewsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NewsApiService> _logger;
    private readonly string _apiKey;
    private const string NewsApiBaseUrl = "https://newsapi.org/v2/";
    private const string DefaultCategory = "business";

    public NewsApiService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<NewsApiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _apiKey = configuration["NewsApi:ApiKey"] ?? string.Empty;
        _httpClient.BaseAddress = new Uri(NewsApiBaseUrl);

        if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Prospera", "1.0"));
        }
    }

    public async Task<FinancialNewsData?> GetLatestNewsAsync(int pageSize = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = "finance OR investment OR stock OR market OR cryptocurrency";
            return await FetchNewsAsync(query, pageSize, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest financial news");
            return new FinancialNewsData { Success = false, FetchedAt = DateTime.UtcNow };
        }
    }

    public async Task<FinancialNewsData?> GetNewsByCategoryAsync(string category, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = category.ToLower() switch
            {
                "stocks" or "equity" => "(stock OR stocks OR equity OR shares OR trading)",
                "crypto" or "cryptocurrency" or "bitcoin" => "(cryptocurrency OR crypto OR bitcoin OR ethereum OR blockchain)",
                "banking" or "bank" => "(banking OR bank OR loan OR credit OR deposit)",
                "real estate" or "property" => "(real estate OR property OR mortgage OR housing)",
                "bonds" or "fixed income" => "(bonds OR fixed income OR yield OR treasury)",
                "forex" or "currency" => "(forex OR currency OR exchange rate OR FX)",
                "commodities" => "(commodities OR gold OR oil OR silver OR metals)",
                _ => category
            };

            return await FetchNewsAsync(query, pageSize, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching news for category {Category}", category);
            return new FinancialNewsData { Success = false, FetchedAt = DateTime.UtcNow };
        }
    }

    public async Task<FinancialNewsData?> SearchNewsAsync(string keyword, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                _logger.LogWarning("Search keyword is empty");
                return new FinancialNewsData { Success = false, FetchedAt = DateTime.UtcNow };
            }

            return await FetchNewsAsync(keyword, pageSize, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching news for keyword {Keyword}", keyword);
            return new FinancialNewsData { Success = false, FetchedAt = DateTime.UtcNow };
        }
    }

    public async Task<FinancialNewsData?> GetCryptoNewsAsync(int pageSize = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = "(cryptocurrency OR crypto OR bitcoin OR ethereum OR blockchain OR NFT)";
            return await FetchNewsAsync(query, pageSize, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching cryptocurrency news");
            return new FinancialNewsData { Success = false, FetchedAt = DateTime.UtcNow };
        }
    }

    public async Task<FinancialNewsData?> GetTrendingNewsAsync(int pageSize = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = "(trending OR market surge OR stock rally OR bull market OR financial news)";
            return await FetchNewsAsync(query, pageSize, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching trending news");
            return new FinancialNewsData { Success = false, FetchedAt = DateTime.UtcNow };
        }
    }

    private async Task<FinancialNewsData?> FetchNewsAsync(string query, int pageSize, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("NewsApi:ApiKey is not configured");
            return new FinancialNewsData
            {
                Success = false,
                FetchedAt = DateTime.UtcNow
            };
        }

        try
        {
            // Limit page size to avoid excessive data
            var limit = Math.Min(pageSize, 100);

            var url = $"everything?q={Uri.EscapeDataString(query)}&sortBy=publishedAt&pageSize={limit}&apiKey={_apiKey}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("News API returned status {StatusCode}. Response: {ResponseBody}", response.StatusCode, errorBody);
                return new FinancialNewsData { Success = false, FetchedAt = DateTime.UtcNow };
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonData = JsonSerializer.Deserialize<JsonElement>(content);

            var newsData = new FinancialNewsData
            {
                FetchedAt = DateTime.UtcNow
            };

            if (jsonData.TryGetProperty("articles", out var articlesElement))
            {
                if (articlesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var article in articlesElement.EnumerateArray())
                    {
                        var newsArticle = ParseNewsArticle(article);
                        if (newsArticle != null)
                        {
                            newsData.Articles.Add(newsArticle);
                        }
                    }
                }
            }

            if (jsonData.TryGetProperty("totalResults", out var totalResultsElement))
            {
                newsData.TotalResults = totalResultsElement.GetInt32();
            }

            newsData.Success = true;
            _logger.LogInformation("Successfully fetched {Count} financial news articles", newsData.Articles.Count);

            return newsData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching news from NewsAPI");
            return new FinancialNewsData { Success = false, FetchedAt = DateTime.UtcNow };
        }
    }

    private NewsArticle? ParseNewsArticle(JsonElement articleElement)
    {
        try
        {
            var article = new NewsArticle();

            if (articleElement.TryGetProperty("title", out var titleElement))
                article.Title = titleElement.GetString() ?? string.Empty;

            if (articleElement.TryGetProperty("description", out var descElement))
                article.Description = descElement.GetString();

            if (articleElement.TryGetProperty("url", out var urlElement))
                article.Url = urlElement.GetString() ?? string.Empty;

            if (articleElement.TryGetProperty("urlToImage", out var imageElement))
                article.Image = imageElement.GetString();

            if (articleElement.TryGetProperty("source", out var sourceElement))
            {
                if (sourceElement.TryGetProperty("name", out var sourceNameElement))
                    article.Source = sourceNameElement.GetString() ?? string.Empty;
            }

            if (articleElement.TryGetProperty("publishedAt", out var publishedElement))
            {
                if (DateTime.TryParse(publishedElement.GetString(), out var publishedDate))
                    article.PublishedAt = publishedDate;
            }

            // Determine sentiment based on keywords in title/description
            article.Sentiment = DetermineSentiment(article.Title, article.Description);

            return article;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing news article");
            return null;
        }
    }

    private string DetermineSentiment(string title, string? description)
    {
        var combinedText = (title + " " + description).ToLower();

        var positivKeywords = new[] { "surge", "rally", "gain", "rise", "bull", "profit", "strong", "recovery", "boom", "growth", "breakthrough" };
        var negativeKeywords = new[] { "crash", "plunge", "fall", "loss", "bear", "decline", "weak", "recession", "slump", "drop", "collapse" };

        var hasPositive = positivKeywords.Any(k => combinedText.Contains(k));
        var hasNegative = negativeKeywords.Any(k => combinedText.Contains(k));

        if (hasPositive && !hasNegative) return "positive";
        if (hasNegative && !hasPositive) return "negative";
        return "neutral";
    }
}
