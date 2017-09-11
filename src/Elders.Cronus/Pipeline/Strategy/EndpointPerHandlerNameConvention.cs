using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Pipeline.Transport.Strategy
{
    public class EndpointPerHandlerNameConvention : IEndpointNameConvention
    {
        IPipelineNameConvention pipelineNameConvention;

        public EndpointPerHandlerNameConvention(IPipelineNameConvention pipelineNameConvention)
        {
            this.pipelineNameConvention = pipelineNameConvention;
        }

        public IEnumerable<EndpointDefinition> GetEndpointDefinition(IEndpointConsumer consumer, SubscriptionMiddleware subscriptionMiddleware)
        {
            Dictionary<string, HashSet<Type>> handlers = new Dictionary<string, HashSet<Type>>();
            var subType = typeof(SubscriptionMiddleware);
            foreach (var item in subscriptionMiddleware.Subscribers)
            {
                var messageHandlerType = subType.GetField("messageHandlerType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(item) as Type;
                if (!handlers.ContainsKey(item.Id))
                    handlers.Add(item.Id, new HashSet<Type>() { });
                foreach (var messageType in item.MessageTypes)
                {
                    handlers[item.Id].Add(messageType);
                }
            }

            List<string> endpointNames = new List<string>();

            foreach (var item in handlers)
            {
                var pipelineName = pipelineNameConvention.GetPipelineName(item.Value.First());
                var watchMessageTypes = item.Value.Distinct().Select(x=>x.GetContractId()).ToList();
                var bc = item.Value.First().GetBoundedContext().BoundedContextNamespace;

                var endpointName = bc + "." + "(" + item.Key + ")";
                if (endpointNames.Contains(endpointName))
                    throw new InvalidOperationException("Duplicatie endpoint name " + endpointName);

                endpointNames.Add(endpointName);
                yield return new EndpointDefinition(pipelineName, endpointName, watchMessageTypes);
            }
        }
    }
}
