using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Pipeline.Config
{
    public class PipelineEventPublisherSettings : PipelinePublisherSetting<IEvent>
    {
        public PipelineEventPublisherSettings()
        {
            PipelineSettings
                .UseEventPipelinePerApplication()
                .UsePortEndpointPerBoundedContext();
        }

        protected override IPublisher<IEvent> BuildPublisher()
        {
            return new PipelinePublisher<IEvent>(Transport.PipelineFactory, GlobalSettings.Serializer);
        }
    }
}
