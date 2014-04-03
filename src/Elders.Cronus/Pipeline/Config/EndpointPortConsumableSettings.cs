using Elders.Cronus.DomainModelling;
using Elders.Cronus.Pipeline.Strategy;

namespace Elders.Cronus.Pipeline.Config
{
    public class EndpointPortConsumableSettings : EndpointConsumerSetting<IEvent>
    {
        public EndpointPortConsumableSettings()
        {
            PipelineSettings.PipelineNameConvention = new EventPipelinePerApplication();
            PipelineSettings.EndpointNameConvention = new PortEndpointPerBoundedContext(PipelineSettings.PipelineNameConvention);
        }

        protected override IEndpointConsumable BuildConsumer()
        {
            ISafeBatchRetryStrategy<IEvent> batchRetryStrategy = ConsumerBatchSize == 1
                    ? new NoRetryStrategy<IEvent>() as ISafeBatchRetryStrategy<IEvent>
                    : new DefaultRetryStrategy<IEvent>() as ISafeBatchRetryStrategy<IEvent>;

            var safeBatchFactory = new SafeBatchWithBatchScopeContextFactory<IEvent>(batchRetryStrategy, ScopeFactory.CreateBatchScope);

            MessageHandlerCollection<IEvent> handlers = new MessageHandlerCollection<IEvent>(safeBatchFactory, 1);
            foreach (var reg in registrations)
            {
                GlobalSettings.Protoreg.RegisterAssembly(reg.Key);

                foreach (var item in reg.Value)
                {
                    if (typeof(IPort).IsAssignableFrom(item.Item1))
                        handlers.RegisterHandler(reg.Key, item.Item1, item.Item2);
                }
            }

            var consumer = new EndpointConsumer<IEvent>(handlers, ScopeFactory, GlobalSettings.Serializer, SuccessStrategy, ErrorStrategy);
            return new EndpointConsumable(Transport.EndpointFactory, consumer);
        }
    }
}
