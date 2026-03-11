using MediatR;
using AutoMapper;
using Prospera.Domain.Entities;
using Prospera.Domain.Enums;
using Prospera.Domain.Interfaces;
using Prospera.Application.DTOs;

namespace Prospera.Application.Features.Transactions.Commands;

/// <summary>
/// Handler for AddTransactionCommand
/// </summary>
public class AddTransactionCommandHandler : IRequestHandler<AddTransactionCommand, TransactionDto>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;

    public AddTransactionCommandHandler(ITransactionRepository transactionRepository, IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _mapper = mapper;
    }

    public async Task<TransactionDto> Handle(AddTransactionCommand request, CancellationToken cancellationToken)
    {
        // Parse transaction type
        if (!Enum.TryParse<TransactionType>(request.Type, out var transactionType))
        {
            transactionType = TransactionType.Expense;
        }

        // Create new transaction
        var transaction = new Transaction(request.Amount, transactionType, request.Description, request.UserId);

        // Add to repository
        await _transactionRepository.AddAsync(transaction);

        // Map and return
        return _mapper.Map<TransactionDto>(transaction);
    }
}
