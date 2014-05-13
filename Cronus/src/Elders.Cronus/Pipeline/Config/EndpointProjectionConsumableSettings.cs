using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Pipeline.Config
{
    public class EndpointProjectionConsumableSettings : EndpointConsumerSetting<IEvent>
    {
        public EndpointProjectionConsumableSettings()
        {
            PipelineSettings
                .UseEventPipelinePerApplication()
                .UseProjectionEndpointPerBoundedContext();
            base.ConsumerBatchSize = 100;

            this.UseDefaultEndpointPostConsumeStrategy();
        }

        protected override IEndpointConsumable<IEvent> BuildConsumer()
        {
            ISafeBatchRetryStrategy<IEvent> batchRetryStrategy = ConsumerBatchSize == 1
                    ? new NoRetryStrategy<IEvent>() as ISafeBatchRetryStrategy<IEvent>
                    : new DefaultRetryStrategy<IEvent>() as ISafeBatchRetryStrategy<IEvent>;

            var safeBatchFactory = new SafeBatchWithBatchScopeContextFactory<IEvent>(batchRetryStrategy, ScopeFactory.CreateBatchScope);

            MessageHandlerCollection<IEvent> handlers = new MessageHandlerCollection<IEvent>(ScopeFactory, safeBatchFactory, ConsumerBatchSize);
            foreach (var reg in registrations)
            {
                GlobalSettings.Protoreg.RegisterAssembly(reg.Key);
                foreach (var item in reg.Value)
                {
                    if (!typeof(IPort).IsAssignableFrom(item.Item1))
                        handlers.RegisterHandler(reg.Key, item.Item1, item.Item2);
                }
            }

            var consumer = new EndpointConsumer<IEvent>(handlers, GlobalSettings.Serializer, PostConsume);
            var consumable = new EndpointConsumable<IEvent>(Transport.EndpointFactory, consumer, GlobalSettings.Serializer, ConsumerBatchSize);
            consumable.NumberOfWorkers = ((IConsumerSettings)this).NumberOfWorkers;
            return consumable;
        }
    }
}
