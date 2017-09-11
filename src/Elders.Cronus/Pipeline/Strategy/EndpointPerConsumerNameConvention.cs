using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Pipeline.Transport.Strategy
{
    public class EndpointPerConsumerNameConvention : EndpointNameConvention
    {
        readonly IPipelineNameConvention pipelineNameConvention;

        public EndpointPerConsumerNameConvention(IPipelineNameConvention pipelineNameConvention)
        {
            this.pipelineNameConvention = pipelineNameConvention;
        }

        public override IEnumerable<EndpointDefinition> GetEndpointDefinition(IEndpointConsumer consumer, SubscriptionMiddleware subscriptionMiddleware)
        {
            var pipeLine = subscriptionMiddleware.Subscribers.Select(x => pipelineNameConvention.GetPipelineName(x.MessageTypes.First())).Distinct();
            if (pipeLine.Count() == 0)
                throw new ArgumentException("Cannot find pipeline to subscribe to.");
            else if (pipeLine.Count() > 1)
                throw new ArgumentException("Cannot subscribe to more than one pipeline. Probably you have mixed ICommand and IEvent messages within a single handler.");

            var watchMessageTypes = subscriptionMiddleware.Subscribers.SelectMany(x => x.MessageTypes).Distinct().Select(x=>x.GetContractId()).ToList();

            EndpointDefinition endpointDefinition = new EndpointDefinition(pipeLine.Single(), consumer.Name, watchMessageTypes);
            yield return endpointDefinition;
        }
    }
}

