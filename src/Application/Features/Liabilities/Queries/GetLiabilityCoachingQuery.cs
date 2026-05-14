using MediatR;
using Prospera.Application.Common.Interfaces;
using Prospera.Application.DTOs;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Features.Liabilities.Queries;

/// <summary>
/// Query to get liability with coaching advice and debt reduction strategy
/// </summary>
public class GetLiabilityCoachingQuery : IRequest<LiabilityDto>
{
    public required Guid UserId { get; set; }
    public required Guid LiabilityId { get; set; }

    /// <summary>
    /// Optional: Skip coaching advice generation (for performance)
    /// </summary>
    public bool IncludeCoaching { get; set; } = true;
}

/// <summary>
/// Handler for GetLiabilityCoachingQuery
/// Retrieves liability details and generates personalized coaching advice
/// </summary>
public class GetLiabilityCoachingQueryHandler : IRequestHandler<GetLiabilityCoachingQuery, LiabilityDto>
{
    private readonly ILiabilityRepository _liabilityRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICoachingService _coachingService;

    public GetLiabilityCoachingQueryHandler(
        ILiabilityRepository liabilityRepository,
        IUserRepository userRepository,
        ICoachingService coachingService)
    {
        _liabilityRepository = liabilityRepository;
        _userRepository = userRepository;
        _coachingService = coachingService;
    }

    public async Task<LiabilityDto> Handle(GetLiabilityCoachingQuery request, CancellationToken cancellationToken)
    {
        // Fetch liability
        var liability = await _liabilityRepository.GetByIdAsync(request.LiabilityId, request.UserId);

        if (liability == null || liability.UserId != request.UserId)
        {
            throw new KeyNotFoundException($"Liability with ID {request.LiabilityId} not found");
        }

        // Map to DTO
        var liabilityDto = new LiabilityDto
        {
            Id = liability.Id,
            Name = liability.Name,
            Amount = liability.Amount,
            Type = liability.Type.ToString(),
            UserId = liability.UserId
        };

        // Generate coaching advice if requested
        if (request.IncludeCoaching)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(request.UserId);
                var userLiabilities = await _liabilityRepository.GetByUserIdAsync(request.UserId);
                var totalLiabilities = userLiabilities.Sum(l => l.Amount);
                var totalAssets = 100000m; // Placeholder - would fetch from asset repository in production

                var debtToIncomeRatio = CalculateDebtToIncomeRatio(user, totalLiabilities);

                liabilityDto.CoachingAdvice = await _coachingService.GenerateLiabilityCoachingAdviceAsync(
                    liability.Id,
                    liability.Name,
                    liability.Type.ToString(),
                    liability.Amount,
                    null, // InterestRate - may not be available in current model
                    null, // MonthlyPayment - may not be available in current model
                    totalAssets - totalLiabilities,
                    totalLiabilities,
                    debtToIncomeRatio,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Failed to generate liability coaching advice: {ex.Message}");
            }
        }

        return liabilityDto;
    }

    private decimal CalculateDebtToIncomeRatio(Prospera.Domain.Entities.User? user, decimal totalLiabilities)
    {
        // Placeholder - would calculate actual DTI ratio from user's income
        return 0m;
    }
}
