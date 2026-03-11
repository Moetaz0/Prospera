using Prospera.Contracts.Enums;

namespace Prospera.Contracts.DTOs.Transaction;

public class AddTransactionRequest
{
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string? Description { get; set; }
}
