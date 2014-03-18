using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Pipeline.Strategy;

namespace NMSD.Cronus.Pipeline.Config
{
    public class PipelineEventPublisherSettings : PipelinePublisherSetting<IEvent>
    {
        public PipelineEventPublisherSettings()
        {
            PipelineSettings.PipelineNameConvention = new EventPipelinePerApplication();
        }

        protected override IPublisher<IEvent> BuildPublisher()
        {
            return new PipelinePublisher<IEvent>(Transport.PipelineFactory, GlobalSettings.Serializer);
        }
    }
}
