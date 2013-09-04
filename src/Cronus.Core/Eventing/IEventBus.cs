using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronus.Core.Eventing
{
    public interface IEventBus  
    {
        void Publish(IEvent @event); 
    }
}
