namespace Prospera.Domain.Exceptions;

public class UserNotFoundException : DomainException
{
    public UserNotFoundException(Guid id)
        : base($"User with ID {id} was not found.")
    {
    }
}
