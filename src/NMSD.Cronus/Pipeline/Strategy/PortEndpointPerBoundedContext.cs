using System;
using System.Collections.Generic;
using System.Linq;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Pipeline.Strategy
{
    public class PortEndpointPerBoundedContext : IEndpointNameConvention
    {
        private IPipelineNameConvention pipelineNameConvention;

        public PortEndpointPerBoundedContext(IPipelineNameConvention pipelineNameConvention)
        {
            this.pipelineNameConvention = pipelineNameConvention;
        }

        public IEnumerable<EndpointDefinition> GetEndpointDefinitions(params Type[] handlerTypes)
        {
            var boundedContext = handlerTypes.First().GetBoundedContext();

            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", handlerTypes.First().Assembly.FullName));

            var endpointName = String.Format("{0}.Ports", boundedContext.BoundedContextNamespace);

            var routingHeaders = (from handlerType in handlerTypes
                                  from handlerMethod in handlerType.GetMethods()
                                  from handlerMethodParameter in handlerMethod.GetParameters()
                                  where handlerMethod.Name == "Handle"
                                  select handlerMethodParameter.ParameterType)
                                  .Distinct()
                                 .ToDictionary<Type, string, object>(key => key.GetContractId(), val => String.Empty);

            yield return new EndpointDefinition(endpointName, routingHeaders, pipelineNameConvention.GetPipelineName(boundedContext));
        }
    }
}
