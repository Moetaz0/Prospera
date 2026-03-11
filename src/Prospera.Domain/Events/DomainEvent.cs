using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prospera.Domain.Events
{
    public abstract class DomainEvent
    {
            public Guid Id { get; private set; }
            public DateTime OccurredOn { get; private set; }
    
            public DomainEvent()
            {
                Id = Guid.NewGuid();
                OccurredOn = DateTime.UtcNow;
        }


    }
}
