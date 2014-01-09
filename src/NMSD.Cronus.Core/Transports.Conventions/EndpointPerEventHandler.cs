using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.Core.Cqrs;

namespace NMSD.Cronus.Core.Transports.Conventions
{
    public class EndpointPerEventHandler : IEventHandlerEndpointConvention
    {
        private IEventPipelineConvention pipelineConvention;

        public EndpointPerEventHandler(IEventPipelineConvention pipelineConvention)
        {
            this.pipelineConvention = pipelineConvention;
        }
        public IEnumerable<EndpointDefinition> GetEndpointDefinitions(params Type[] handlerTypes)
        {
            foreach (var handlerType in handlerTypes)
            {
                var boundedContext = handlerType.GetAssemblyAttribute<BoundedContextAttribute>();

                if (boundedContext == null)
                    throw new Exception(String.Format(@"The assembly containing message type '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", handlerType.FullName));

                string endpointName = String.Format("{0}.{1}", boundedContext.BoundedContextNamespace, handlerType.Name);

                var eventTypes = handlerType.GetMethods().Where(x => x.Name == "Handle").SelectMany(x => x.GetParameters().Select(y => y.ParameterType));
                var eventIds = eventTypes.Select(x => new Guid((x.GetCustomAttribute(typeof(DataContractAttribute), false) as DataContractAttribute).Name));
                yield return new EndpointDefinition(endpointName, new List<Guid>(eventIds), pipelineConvention.GetPipelineName(eventTypes.First()));
            }
        }
    }
}
