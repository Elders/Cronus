using System.Linq;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Pipeline.Strategy;

namespace NMSD.Cronus.Pipeline.Config
{
    public class EndpointEventConsumableSettings : EndpointConsumerSetting<IEvent>
    {
        public EndpointEventConsumableSettings()
        {
            PipelineSettings.PipelineNameConvention = new EventPipelinePerApplication();
            PipelineSettings.EndpointNameConvention = new EventHandlerEndpointPerBoundedContext(PipelineSettings.PipelineNameConvention);
        }

        protected override IEndpointConsumable BuildConsumer()
        {
            MessageHandlerCollection<IEvent> handlers = new MessageHandlerCollection<IEvent>();
            foreach (var reg in registrations)
            {
                GlobalSettings.Protoreg.RegisterCommonType(reg.Key);
                foreach (var item in reg.Value)
                {
                    handlers.RegisterHandler(reg.Key, item.Item1, item.Item2);
                }
            }

            var consumer = new EndpointConsumer<IEvent>(handlers, ScopeFactory, GlobalSettings.Serializer);
            return new EndpointConsumable(Transport.EndpointFactory, consumer);
        }
    }
}
