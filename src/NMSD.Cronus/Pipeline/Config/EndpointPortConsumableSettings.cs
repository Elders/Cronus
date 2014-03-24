using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Pipeline.Strategy;

namespace NMSD.Cronus.Pipeline.Config
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
            MessageHandlerCollection<IEvent> handlers = new MessageHandlerCollection<IEvent>(1);
            foreach (var reg in registrations)
            {
                GlobalSettings.Protoreg.RegisterAssembly(reg.Key);

                foreach (var item in reg.Value)
                {
                    if (typeof(IPort).IsAssignableFrom(item.Item1))
                        handlers.RegisterHandler(reg.Key, item.Item1, item.Item2);
                }
            }

            var consumer = new EndpointConsumer<IEvent>(handlers, ScopeFactory, GlobalSettings.Serializer);
            return new EndpointConsumable(Transport.EndpointFactory, consumer);
        }
    }
}
