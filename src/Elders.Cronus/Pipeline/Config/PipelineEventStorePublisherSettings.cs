using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Pipeline.Strategy;

namespace Elders.Cronus.Pipeline.Config
{
    public class PipelineEventStorePublisherSettings : PipelinePublisherSetting<DomainMessageCommit>
    {
        public PipelineEventStorePublisherSettings()
        {
            PipelineSettings.PipelineNameConvention = new EventStorePipelinePerApplication();
        }

        protected override IPublisher<DomainMessageCommit> BuildPublisher()
        {
            return new EventStorePublisher(Transport.PipelineFactory, GlobalSettings.Serializer);
        }
    }
}
