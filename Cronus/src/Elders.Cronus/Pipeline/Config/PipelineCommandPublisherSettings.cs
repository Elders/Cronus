using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Pipeline.Config
{
    public class PipelineCommandPublisherSettings : PipelinePublisherSetting<ICommand>
    {
        public PipelineCommandPublisherSettings()
        {
            PipelineSettings
                .UseCommandPipelinePerApplication()
                .UseCommandHandlerEndpointPerBoundedContext();
        }

        protected override IPublisher<ICommand> BuildPublisher()
        {
            return new PipelinePublisher<ICommand>(Transport.PipelineFactory, GlobalSettings.Serializer);
        }
    }
}
