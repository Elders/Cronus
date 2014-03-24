using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Pipeline.Strategy;

namespace NMSD.Cronus.Pipeline.Config
{
    public class EndpointProjectionConsumableSettings : EndpointConsumerSetting<IEvent>
    {
        public EndpointProjectionConsumableSettings()
        {
            PipelineSettings.PipelineNameConvention = new EventPipelinePerApplication();
            PipelineSettings.EndpointNameConvention = new ProjectionEndpointPerBoundedContext(PipelineSettings.PipelineNameConvention);
        }

        protected override IEndpointConsumable BuildConsumer()
        {
            MessageHandlerCollection<IEvent> handlers = new MessageHandlerCollection<IEvent>(100);
            foreach (var reg in registrations)
            {
                GlobalSettings.Protoreg.RegisterAssembly(reg.Key);
                foreach (var item in reg.Value)
                {
                    if (!typeof(IPort).IsAssignableFrom(item.Item1))
                        handlers.RegisterHandler(reg.Key, item.Item1, item.Item2);
                }
            }

            var consumer = new EndpointConsumer<IEvent>(handlers, ScopeFactory, GlobalSettings.Serializer);
            return new EndpointConsumable(Transport.EndpointFactory, consumer);
        }
    }
}
