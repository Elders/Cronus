using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Pipeline.Strategy;

namespace Elders.Cronus.Pipeline.Config
{
    public class EndpointEventStoreConsumableSettings : EndpointConsumerSetting<IEvent>
    {
        public EndpointEventStoreConsumableSettings()
        {
            PipelineSettings.PipelineNameConvention = new EventStorePipelinePerApplication();
            PipelineSettings.EndpointNameConvention = new EventStoreEndpointPerBoundedContext(PipelineSettings.PipelineNameConvention);
        }

        protected override IEndpointConsumable BuildConsumer()
        {
            SuccessStrategy = new EventStorePublishEventsOnSuccessPersist(GlobalSettings.EventPublisher);
            var consumer = new EndpointConsumer<DomainMessageCommit>(GlobalSettings.EventStoreHandlers[BoundedContext], ScopeFactory, GlobalSettings.Serializer, SuccessStrategy, ErrorStrategy);
            return new EndpointConsumable(Transport.EndpointFactory, consumer);
        }
    }
}
