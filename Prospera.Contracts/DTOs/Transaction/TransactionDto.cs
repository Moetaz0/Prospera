using Prospera.Contracts.Enums;

namespace Prospera.Contracts.DTOs.Transaction;

public class TransactionDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public string? Description { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
