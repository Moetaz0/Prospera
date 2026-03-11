using MediatR;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Recommendations.Commands;

/// <summary>
/// Command to generate AI-powered investment recommendations for a user
/// </summary>
public class GenerateRecommendationCommand : IRequest<InvestmentRecommendationDto>
{
    public required Guid UserId { get; set; }
    public required string AnalysisContext { get; set; }
}
