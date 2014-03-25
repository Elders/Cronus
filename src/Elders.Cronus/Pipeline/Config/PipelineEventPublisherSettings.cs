using Elders.Cronus.DomainModelling;
using Elders.Cronus.Pipeline.Strategy;

namespace Elders.Cronus.Pipeline.Config
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
