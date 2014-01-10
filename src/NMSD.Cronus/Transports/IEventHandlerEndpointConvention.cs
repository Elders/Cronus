using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NMSD.Cronus.Transports.Conventions
{
    public interface IEventHandlerEndpointConvention
    {
        IEnumerable<EndpointDefinition> GetEndpointDefinitions(params Type[] handlerTypes);
    }
}