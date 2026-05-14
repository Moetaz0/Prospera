using MediatR;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Features.News.Queries;

public class GetCachedNewsQuery : IRequest<GetCachedNewsResponse>
{
    /// <summary>
    /// News type: "business", "crypto", "trending" or empty for all
    /// </summary>
    public string? NewsType { get; set; }
    public int PageSize { get; set; } = 20;
}

public class GetCachedNewsResponse
{
    public bool Success { get; set; }
    public List<CachedNewsItemResponse> Articles { get; set; } = new();
    public int TotalResults { get; set; }
    public DateTime CacheGeneratedAt { get; set; }
}

public class CachedNewsItemResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? Category { get; set; }
    public DateTime PublishedAt { get; set; }
    public string? Sentiment { get; set; }
    public string NewsType { get; set; } = string.Empty;
    public DateTime CachedAt { get; set; }
}

public class GetCachedNewsQueryHandler : IRequestHandler<GetCachedNewsQuery, GetCachedNewsResponse>
{
    private readonly INewsRepository _newsRepository;

    public GetCachedNewsQueryHandler(INewsRepository newsRepository)
    {
        _newsRepository = newsRepository;
    }

    public async Task<GetCachedNewsResponse> Handle(GetCachedNewsQuery request, CancellationToken cancellationToken)
    {
        var response = new GetCachedNewsResponse
        {
            CacheGeneratedAt = DateTime.UtcNow
        };

        IEnumerable<Prospera.Domain.Entities.CachedNewsArticle> articles;

        if (string.IsNullOrWhiteSpace(request.NewsType))
        {
            // Get all news
            articles = await _newsRepository.GetRecentAsync(days: 7, take: request.PageSize, cancellationToken);
        }
        else
        {
            // Get specific type
            articles = await _newsRepository.GetByTypeAsync(request.NewsType, take: request.PageSize, cancellationToken);
        }

        response.Success = true;
        response.TotalResults = articles.Count();
        response.Articles = articles.Select(a => new CachedNewsItemResponse
        {
            Id = a.Id,
            Title = a.Title,
            Description = a.Description,
            Url = a.Url,
            Image = a.Image,
            Source = a.Source,
            Category = a.Category,
            PublishedAt = a.PublishedAt,
            Sentiment = a.Sentiment,
            NewsType = a.NewsType,
            CachedAt = a.CachedAt
        }).ToList();

        return response;
    }
}
