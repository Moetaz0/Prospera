namespace Prospera.Domain.Exceptions;

public class InvalidTransactionException : DomainException
{
    public InvalidTransactionException(string message)
        : base(message)
    {
    }
}
