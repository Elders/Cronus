using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;

namespace Elders.Cronus.Pipeline.Config
{
    public class PipelineEventStorePublisherSettings : PipelinePublisherSetting<DomainMessageCommit>
    {
        public PipelineEventStorePublisherSettings()
        {
            PipelineSettings
                .UseEventStorePipelinePerApplication()
                .UseEventStoreEndpointPerBoundedContext();
        }

        protected override IPublisher<DomainMessageCommit> BuildPublisher()
        {
            return new EventStorePublisher(Transport.PipelineFactory, GlobalSettings.Serializer);
        }
    }
}
