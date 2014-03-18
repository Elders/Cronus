using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Pipeline.Strategy;

namespace NMSD.Cronus.Pipeline.Config
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
