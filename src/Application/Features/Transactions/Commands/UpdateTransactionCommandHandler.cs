using AutoMapper;
using MediatR;
using Prospera.Application.DTOs;
using Prospera.Domain.Enums;
using Prospera.Domain.Interfaces;

namespace Prospera.Application.Features.Transactions.Commands;

public class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand, TransactionDto?>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;

    public UpdateTransactionCommandHandler(ITransactionRepository transactionRepository, IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _mapper = mapper;
    }

    public async Task<TransactionDto?> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, request.UserId);
        if (transaction is null)
        {
            return null;
        }

        if (!Enum.TryParse<TransactionType>(request.Type, out var type))
        {
            type = TransactionType.Expense;
        }

        transaction.UpdateDetails(request.Amount, type, request.Description);
        await _transactionRepository.UpdateAsync(transaction);

        return _mapper.Map<TransactionDto>(transaction);
    }
}
