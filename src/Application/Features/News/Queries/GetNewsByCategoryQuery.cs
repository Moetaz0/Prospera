using MediatR;
using Prospera.Application.Common.Interfaces;

namespace Prospera.Application.Features.News.Queries;

public class GetNewsByCategoryQuery : IRequest<GetNewsByCategoryResponse>
{
    public string Category { get; set; } = string.Empty;
    public int PageSize { get; set; } = 10;
}

public class GetNewsByCategoryResponse
{
    public bool Success { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<NewsItemResponse> Articles { get; set; } = new();
    public int TotalResults { get; set; }
    public DateTime FetchedAt { get; set; }
}

public class GetNewsByCategoryQueryHandler : IRequestHandler<GetNewsByCategoryQuery, GetNewsByCategoryResponse>
{
    private readonly IFinancialNewsService _newsService;

    public GetNewsByCategoryQueryHandler(IFinancialNewsService newsService)
    {
        _newsService = newsService;
    }

    public async Task<GetNewsByCategoryResponse> Handle(GetNewsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var newsData = await _newsService.GetNewsByCategoryAsync(request.Category, request.PageSize, cancellationToken);

        if (newsData == null || !newsData.Success)
        {
            return new GetNewsByCategoryResponse
            {
                Success = false,
                Category = request.Category,
                FetchedAt = DateTime.UtcNow
            };
        }

        var response = new GetNewsByCategoryResponse
        {
            Success = true,
            Category = request.Category,
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
