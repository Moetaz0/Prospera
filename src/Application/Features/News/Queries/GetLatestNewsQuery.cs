using MediatR;
using Prospera.Application.Common.Interfaces;

namespace Prospera.Application.Features.News.Queries;

public class GetLatestNewsQuery : IRequest<GetLatestNewsResponse>
{
    public int PageSize { get; set; } = 10;
}

public class GetLatestNewsResponse
{
    public bool Success { get; set; }
    public List<NewsItemResponse> Articles { get; set; } = new();
    public int TotalResults { get; set; }
    public DateTime FetchedAt { get; set; }
}

public class NewsItemResponse
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? Category { get; set; }
    public DateTime PublishedAt { get; set; }
    public string? Sentiment { get; set; }
}

public class GetLatestNewsQueryHandler : IRequestHandler<GetLatestNewsQuery, GetLatestNewsResponse>
{
    private readonly IFinancialNewsService _newsService;

    public GetLatestNewsQueryHandler(IFinancialNewsService newsService)
    {
        _newsService = newsService;
    }

    public async Task<GetLatestNewsResponse> Handle(GetLatestNewsQuery request, CancellationToken cancellationToken)
    {
        var newsData = await _newsService.GetLatestNewsAsync(request.PageSize, cancellationToken);

        if (newsData == null || !newsData.Success)
        {
            return new GetLatestNewsResponse
            {
                Success = false,
                FetchedAt = DateTime.UtcNow
            };
        }

        var response = new GetLatestNewsResponse
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
