using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Pipeline.Strategy;

namespace NMSD.Cronus.Pipeline.Config
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
            MessageHandlerCollection<ICommand> handlers = new MessageHandlerCollection<ICommand>();
            foreach (var reg in registrations)
            {
                GlobalSettings.Protoreg.RegisterCommonType(reg.Key);
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
