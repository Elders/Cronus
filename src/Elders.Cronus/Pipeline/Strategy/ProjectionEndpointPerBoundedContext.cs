using System;
using System.Linq;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Pipeline.Strategy
{
    public class ProjectionEndpointPerBoundedContext : IProjectionEndpointNameConvention
    {
        private IEventPipelineNameConvention pipelineNameConvention;

        public ProjectionEndpointPerBoundedContext(IEventPipelineNameConvention pipelineNameConvention)
        {
            this.pipelineNameConvention = pipelineNameConvention;
        }

        public EndpointDefinition GetEndpointDefinition(params Type[] handlerTypes)
        {
            var boundedContext = handlerTypes.First().GetBoundedContext();

            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", handlerTypes.First().Assembly.FullName));

            var endpointName = String.Format("{0}.Projections", boundedContext.BoundedContextNamespace);

            var routingHeaders = (from handlerType in handlerTypes
                                  from handlerMethod in handlerType.GetMethods()
                                  from handlerMethodParameter in handlerMethod.GetParameters()
                                  where handlerMethod.Name == "Handle"
                                  select handlerMethodParameter.ParameterType)
                                  .Distinct()
                                 .ToDictionary<Type, string, object>(key => key.GetContractId(), val => String.Empty);

            EndpointDefinition endpointDefinition = new EndpointDefinition(pipelineNameConvention.GetPipelineName(boundedContext), endpointName, routingHeaders);
            return endpointDefinition;
        }
    }
}