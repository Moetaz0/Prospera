using AutoMapper;
using Prospera.Domain.Entities;
using Prospera.Application.DTOs;
using Prospera.Application.DTOs.Coaching;

namespace Prospera.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.NetWorth, opt => opt.MapFrom(src => src.CalculateNetWorth()))
            .ReverseMap();

        // Asset mappings
        CreateMap<Asset, AssetDto>().ReverseMap();

        // Liability mappings
        CreateMap<Liability, LiabilityDto>().ReverseMap();

        // Transaction mappings
        CreateMap<Transaction, TransactionDto>().ReverseMap();

        // InvestmentRecommendation mappings
        CreateMap<InvestmentRecommendation, InvestmentRecommendationDto>().ReverseMap();

        // FinancialMetrics mappings
        CreateMap<FinancialMetrics, FinancialMetricsDto>().ReverseMap();

        // Coaching Session mappings
        CreateMap<CoachingSession, CoachingSessionDto>().ReverseMap();
        CreateMap<CoachingActionItem, ActionItemDto>().ReverseMap();
        CreateMap<CoachingMilestone, MilestoneDto>().ReverseMap();
    }
}
