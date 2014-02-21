using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Transports.Conventions
{
    public class EventHandlerPerEndpoint : IEndpointNameConvention
    {
        private IPipelineNameConvention pipelineNameConvention;

        public EventHandlerPerEndpoint(IPipelineNameConvention pipelineNameConvention)
        {
            this.pipelineNameConvention = pipelineNameConvention;
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
                var eventIds = eventTypes.Select(x => (x.GetCustomAttribute(typeof(DataContractAttribute), false) as DataContractAttribute).Name);
                var acceptanceHeaders = new Dictionary<string, object>();
                foreach (var id in eventIds)
                {
                    acceptanceHeaders[id] = String.Empty;
                }
                yield return new EndpointDefinition(endpointName, acceptanceHeaders, pipelineNameConvention.GetPipelineName(eventTypes.First()));
            }
        }
    }
}