using MediatR;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Features.Liabilities.Commands;

public class DeleteLiabilityCommandHandler : IRequestHandler<DeleteLiabilityCommand, bool>
{
    private readonly ILiabilityRepository _liabilityRepository;

    public DeleteLiabilityCommandHandler(ILiabilityRepository liabilityRepository)
    {
        _liabilityRepository = liabilityRepository;
    }

    public async Task<bool> Handle(DeleteLiabilityCommand request, CancellationToken cancellationToken)
    {
        var liability = await _liabilityRepository.GetByIdAsync(request.LiabilityId, request.UserId);
        if (liability is null)
        {
            return false;
        }

        await _liabilityRepository.DeleteAsync(request.LiabilityId);
        return true;
    }
}
