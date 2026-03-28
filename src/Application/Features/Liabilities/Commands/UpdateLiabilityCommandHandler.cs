using AutoMapper;
using MediatR;
using Prospera.Application.DTOs;
using Prospera.Domain.Enums;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Features.Liabilities.Commands;

public class UpdateLiabilityCommandHandler : IRequestHandler<UpdateLiabilityCommand, LiabilityDto?>
{
    private readonly ILiabilityRepository _liabilityRepository;
    private readonly IMapper _mapper;

    public UpdateLiabilityCommandHandler(ILiabilityRepository liabilityRepository, IMapper mapper)
    {
        _liabilityRepository = liabilityRepository;
        _mapper = mapper;
    }

    public async Task<LiabilityDto?> Handle(UpdateLiabilityCommand request, CancellationToken cancellationToken)
    {
        var liability = await _liabilityRepository.GetByIdAsync(request.LiabilityId, request.UserId);
        if (liability is null)
        {
            return null;
        }

        if (!Enum.TryParse<LiabilityType>(request.Type, out var type))
        {
            type = LiabilityType.Other;
        }

        liability.UpdateDetails(request.Name, request.Amount, type);
        await _liabilityRepository.UpdateAsync(liability);

        return _mapper.Map<LiabilityDto>(liability);
    }
}
