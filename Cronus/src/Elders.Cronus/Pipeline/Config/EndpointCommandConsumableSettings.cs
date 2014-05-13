using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Pipeline.Config
{
    public class EndpointCommandConsumableSettings : EndpointConsumerSetting<ICommand>
    {
        public EndpointCommandConsumableSettings()
        {
            PipelineSettings
                .UseCommandPipelinePerApplication()
                .UseCommandHandlerEndpointPerBoundedContext();

            this.UseDefaultEndpointPostConsumeStrategy();
        }

        protected override IEndpointConsumable<ICommand> BuildConsumer()
        {
            ISafeBatchRetryStrategy<ICommand> batchRetryStrategy = ConsumerBatchSize == 1
                    ? new NoRetryStrategy<ICommand>() as ISafeBatchRetryStrategy<ICommand>
                    : new DefaultRetryStrategy<ICommand>() as ISafeBatchRetryStrategy<ICommand>;

            var safeBatchFactory = new SafeBatchWithBatchScopeContextFactory<ICommand>(batchRetryStrategy, ScopeFactory.CreateBatchScope);

            MessageHandlerCollection<ICommand> handlers = new MessageHandlerCollection<ICommand>(ScopeFactory, safeBatchFactory, ConsumerBatchSize);
            foreach (var reg in registrations)
            {
                GlobalSettings.Protoreg.RegisterAssembly(reg.Key);
                foreach (var item in reg.Value)
                {
                    handlers.RegisterHandler(reg.Key, item.Item1, item.Item2);
                }
            }

            var consumer = new EndpointConsumer<ICommand>(handlers, GlobalSettings.Serializer, PostConsume);
            var consumable = new EndpointConsumable<ICommand>(Transport.EndpointFactory, consumer, GlobalSettings.Serializer, ConsumerBatchSize);
            consumable.NumberOfWorkers = ((IConsumerSettings)this).NumberOfWorkers;
            return consumable;
        }
    }
}