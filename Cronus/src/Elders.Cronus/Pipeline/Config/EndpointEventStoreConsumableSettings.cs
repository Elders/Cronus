using Elders.Cronus.EventSourcing;

namespace Elders.Cronus.Pipeline.Config
{
    public class EndpointEventStoreConsumableSettings : EndpointConsumerSetting<DomainMessageCommit>
    {
        public EndpointEventStoreConsumableSettings()
        {
            PipelineSettings
                .UseEventStorePipelinePerApplication()
                .UseEventStoreEndpointPerBoundedContext();

            this.UseDefaultEndpointPostConsumeStrategy();
            this.OnConsumeSuccess(() => new EndpointPostConsumeStrategy.EventStorePublishEventsOnSuccessPersist(GlobalSettings.EventPublisher));
        }

        protected override IEndpointConsumable<DomainMessageCommit> BuildConsumer()
        {
            var consumer = new EndpointConsumer<DomainMessageCommit>(GlobalSettings.EventStoreHandlers[BoundedContext], GlobalSettings.Serializer, PostConsume);
            var consumable = new EndpointConsumable<DomainMessageCommit>(Transport.EndpointFactory, consumer, GlobalSettings.Serializer, ConsumerBatchSize);
            consumable.NumberOfWorkers = ((IConsumerSettings)this).NumberOfWorkers;
            return consumable;
        }
    }
}
