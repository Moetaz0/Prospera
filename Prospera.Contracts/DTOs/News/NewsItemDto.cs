using System.Text.Json.Serialization;

namespace Prospera.Contracts.DTOs.News;

public class NewsItemDto
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("publishedAt")]
    public DateTime PublishedAt { get; set; }

    [JsonPropertyName("sentiment")]
    public string? Sentiment { get; set; } // positive, negative, neutral
}

public class FinancialNewsResponseDto
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("totalResults")]
    public int TotalResults { get; set; }

    [JsonPropertyName("articles")]
    public List<NewsItemDto> Articles { get; set; } = new();

    [JsonPropertyName("fetchedAt")]
    public DateTime FetchedAt { get; set; }
}
