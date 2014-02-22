using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Transports.Conventions
{
    public class CommandHandlersPerBoundedContext : IEndpointNameConvention
    {
        private IPipelineNameConvention pipelineConvention;

        public CommandHandlersPerBoundedContext(IPipelineNameConvention pipelineConvention)
        {
            this.pipelineConvention = pipelineConvention;
        }

        public IEnumerable<EndpointDefinition> GetEndpointDefinitions(params Type[] handlerTypes)
        {
            var assembliesContainingHandlersCount = handlerTypes.GroupBy(x => x.Assembly).Count();
            if (assembliesContainingHandlersCount > 1)
                throw new ArgumentException("Handler types must not come from different asssemblies");

            var boundedContext = handlerTypes.First().GetBoundedContext();

            if (boundedContext == null)
                throw new Exception(String.Format(@"The assembly '{0}' is missing a BoundedContext attribute in AssemblyInfo.cs! Example: [BoundedContext(""Company.Product.BoundedContext"")]", handlerTypes.First().Assembly.FullName));

            string endpointName = String.Format("{0}.Commands", boundedContext.BoundedContextNamespace);
            Dictionary<string, object> endpointMessagesDefinitions = new Dictionary<string, object>();
            IEnumerable<Type> messageTypes = new List<Type>();
            foreach (Type handlerType in handlerTypes)
            {
                messageTypes = handlerType.GetMethods().Where(x => x.Name == "Handle").SelectMany(x => x.GetParameters().Select(y => y.ParameterType));
                var eventIds = messageTypes.Select(x => (x.GetCustomAttribute(typeof(DataContractAttribute), false) as DataContractAttribute).Name);
                foreach (var id in eventIds)
                {
                    endpointMessagesDefinitions[id] = String.Empty;
                }
            }
            yield return new EndpointDefinition(endpointName, endpointMessagesDefinitions, pipelineConvention.GetPipelineName(messageTypes.First()));
        }
    }
}