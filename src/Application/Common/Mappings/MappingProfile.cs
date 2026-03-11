using AutoMapper;
using Prospera.Domain.Entities;
using Prospera.Application.DTOs;

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
    }
}
