using Elders.Cronus.DomainModelling;
using Elders.Protoreg;

namespace Elders.Cronus.Pipeline
{
    public class PipelinePublisher<T> : Publisher<T> where T : IMessage
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(PipelinePublisher<T>));

        private readonly IPipelineFactory<IPipeline> pipelineFactory;

        private readonly ProtoregSerializer serializer;

        public PipelinePublisher(IPipelineFactory<IPipeline> pipelineFactory, ProtoregSerializer serializer)
        {
            this.pipelineFactory = pipelineFactory;
            this.serializer = serializer;
        }

        protected override bool PublishInternal(T message)
        {
            pipelineFactory
                .GetPipeline(message.GetType())
                .Push(message.AsEndpointMessage(serializer));
            return true;
        }
    }
}