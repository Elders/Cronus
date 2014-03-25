using Elders.Cronus.DomainModelling;
using Elders.Cronus.Pipeline.Strategy;

namespace Elders.Cronus.Pipeline.Config
{
    public class EndpointCommandConsumableSettings : EndpointConsumerSetting<ICommand>
    {

        public EndpointCommandConsumableSettings()
        {
            PipelineSettings.PipelineNameConvention = new CommandPipelinePerApplication();
            PipelineSettings.EndpointNameConvention = new CommandHandlerEndpointPerBoundedContext(PipelineSettings.PipelineNameConvention);
        }

        protected override IEndpointConsumable BuildConsumer()
        {
            MessageHandlerCollection<ICommand> handlers = new MessageHandlerCollection<ICommand>(1);
            foreach (var reg in registrations)
            {
                GlobalSettings.Protoreg.RegisterAssembly(reg.Key);
                foreach (var item in reg.Value)
                {
                    handlers.RegisterHandler(reg.Key, item.Item1, item.Item2);
                }
            }

            var consumer = new EndpointConsumer<ICommand>(handlers, ScopeFactory, GlobalSettings.Serializer);
            return new EndpointConsumable(Transport.EndpointFactory, consumer);
        }
    }
}
