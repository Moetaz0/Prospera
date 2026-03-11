using MediatR;
using AutoMapper;
using Prospera.Domain.Entities;
using Prospera.Domain.Enums;
using Prospera.Domain.Interfaces;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Liabilities.Commands;

/// <summary>
/// Handler for AddLiabilityCommand
/// </summary>
public class AddLiabilityCommandHandler : IRequestHandler<AddLiabilityCommand, LiabilityDto>
{
    private readonly ILiabilityRepository _liabilityRepository;
    private readonly IMapper _mapper;

    public AddLiabilityCommandHandler(ILiabilityRepository liabilityRepository, IMapper mapper)
    {
        _liabilityRepository = liabilityRepository;
        _mapper = mapper;
    }

    public async Task<LiabilityDto> Handle(AddLiabilityCommand request, CancellationToken cancellationToken)
    {
        // Parse liability type
        if (!Enum.TryParse<LiabilityType>(request.Type, out var liabilityType))
        {
            liabilityType = LiabilityType.Other;
        }

        // Create new liability
        var liability = new Liability(request.Name, request.Amount, liabilityType, request.UserId);

        // Add to repository
        await _liabilityRepository.AddAsync(liability);

        // Map and return
        return _mapper.Map<LiabilityDto>(liability);
    }
}
