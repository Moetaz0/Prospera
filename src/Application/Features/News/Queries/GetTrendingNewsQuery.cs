using MediatR;
using Prospera.Application.Common.Interfaces;

namespace Prospera.Application.Features.News.Queries;

public class GetTrendingNewsQuery : IRequest<GetTrendingNewsResponse>
{
    public int PageSize { get; set; } = 10;
}

public class GetTrendingNewsResponse
{
    public bool Success { get; set; }
    public List<NewsItemResponse> Articles { get; set; } = new();
    public int TotalResults { get; set; }
    public DateTime FetchedAt { get; set; }
}

public class GetTrendingNewsQueryHandler : IRequestHandler<GetTrendingNewsQuery, GetTrendingNewsResponse>
{
    private readonly IFinancialNewsService _newsService;

    public GetTrendingNewsQueryHandler(IFinancialNewsService newsService)
    {
        _newsService = newsService;
    }

    public async Task<GetTrendingNewsResponse> Handle(GetTrendingNewsQuery request, CancellationToken cancellationToken)
    {
        var newsData = await _newsService.GetTrendingNewsAsync(request.PageSize, cancellationToken);

        if (newsData == null || !newsData.Success)
        {
            return new GetTrendingNewsResponse
            {
                Success = false,
                FetchedAt = DateTime.UtcNow
            };
        }

        var response = new GetTrendingNewsResponse
        {
            Success = true,
            TotalResults = newsData.TotalResults,
            FetchedAt = newsData.FetchedAt,
            Articles = newsData.Articles.Select(a => new NewsItemResponse
            {
                Title = a.Title,
                Description = a.Description,
                Url = a.Url,
                Image = a.Image,
                Source = a.Source,
                Category = a.Category,
                PublishedAt = a.PublishedAt,
                Sentiment = a.Sentiment
            }).ToList()
        };

        return response;
    }
}
