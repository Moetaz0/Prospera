using AutoMapper;
using Prospera.Contracts.DTOs.Asset;
using Prospera.Contracts.DTOs.Liability;
using Prospera.Contracts.DTOs.Transaction;
using Prospera.Contracts.DTOs.FinancialMetrics;
using Prospera.Contracts.DTOs.User;
using Prospera.Contracts.DTOs.Recommendations;

namespace Prospera.Contracts.Mappings;

/// <summary>
/// AutoMapper profile for Contracts DTOs.
/// Domain-to-Contract mapping should be handled in the Application or API layer.
/// This profile can be used for DTO-to-DTO transformations if needed.
/// </summary>
public class ContractMappingProfile : Profile
{
    public ContractMappingProfile()
    {
        // DTO-to-DTO mappings can be added here as needed
        // Domain entity mappings should be handled in Application or API layers
    }
}
