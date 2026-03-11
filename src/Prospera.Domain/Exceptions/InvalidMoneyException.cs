

namespace Prospera.Domain.Exceptions
{
    internal class InvalidMoneyException : DomainException
    {
        public InvalidMoneyException() : base("Money amount cannot be negative.")
        {

        }

    }
}
