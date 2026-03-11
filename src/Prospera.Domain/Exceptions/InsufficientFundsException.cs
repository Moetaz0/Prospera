using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prospera.Domain.Exceptions
{
    internal class InsufficientFundsException : DomainException
    {
        public InsufficientFundsException()  : base("Insufficient funds for this operation.")
        {
        }
    }
}
