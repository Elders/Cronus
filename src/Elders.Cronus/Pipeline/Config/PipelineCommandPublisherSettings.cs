using Elders.Cronus.DomainModelling;
using Elders.Cronus.Pipeline.Strategy;

namespace Elders.Cronus.Pipeline.Config
{
    public class PipelineCommandPublisherSettings : PipelinePublisherSetting<ICommand>
    {
        public PipelineCommandPublisherSettings()
        {
            PipelineSettings.PipelineNameConvention = new CommandPipelinePerApplication();
        }

        protected override IPublisher<ICommand> BuildPublisher()
        {
            return new PipelinePublisher<ICommand>(Transport.PipelineFactory, GlobalSettings.Serializer);
        }
    }
}
