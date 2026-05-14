using MediatR;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Features.Transactions.Commands;

public class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand, bool>
{
    private readonly ITransactionRepository _transactionRepository;

    public DeleteTransactionCommandHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<bool> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, request.UserId);
        if (transaction is null)
        {
            return false;
        }

        await _transactionRepository.DeleteAsync(request.TransactionId);
        return true;
    }
}
