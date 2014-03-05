using System;
using System.Collections.Generic;
using System.Linq;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Transports;
using NMSD.Cronus.Transports.Conventions;

namespace NMSD.Cronus.Pipelining.Transport.Strategy
{
    public class EventHandlerEndpointPerBoundedContext : IEndpointNameConvention
    {
        private IPipelineNameConvention pipelineNameConvention;

        public EventHandlerEndpointPerBoundedContext(IPipelineNameConvention pipelineNameConvention)
        {
            this.pipelineNameConvention = pipelineNameConvention;
        }

        public IEnumerable<EndpointDefinition> GetEndpointDefinitions(params Type[] handlerTypes)
        {
            var boundedContext = handlerTypes.First().GetBoundedContext();

            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", handlerTypes.First().Assembly.FullName));

            var endpointName = String.Format("{0}.EventHandlers", boundedContext.BoundedContextNamespace);

            var routingHeaders = (from handlerType in handlerTypes
                                  from handlerMethod in handlerType.GetMethods()
                                  from handlerMethodParameter in handlerMethod.GetParameters()
                                  where handlerMethod.Name == "Handle"
                                  select handlerMethodParameter.ParameterType)
                                 .ToDictionary<Type, string, object>(key => key.GetContractId(), val => String.Empty);

            yield return new EndpointDefinition(endpointName, routingHeaders, pipelineNameConvention.GetPipelineName(boundedContext));
        }
    }
}