using System;
using System.Collections.Concurrent;

namespace Elders.Cronus.Pipeline.Transport.RabbitMQ
{
    public class RabbitMqPipelineFactory : IPipelineFactory<IRabbitMqPipeline>
    {
        private readonly IPipelineNameConvention nameConvention;

        private ConcurrentDictionary<string, IRabbitMqPipeline> pipes = new ConcurrentDictionary<string, IRabbitMqPipeline>();

        private RabbitMqSession session;

        public RabbitMqPipelineFactory(RabbitMqSession session, IPipelineNameConvention nameConvention)
        {
            this.nameConvention = nameConvention;
            this.session = session;
        }

        public IRabbitMqPipeline GetPipeline(string pipelineName)
        {
            if (!pipes.ContainsKey(pipelineName))
            {
                IRabbitMqPipeline pipeline = new RabbitMqPipeline(pipelineName, session, RabbitMqPipeline.PipelineType.Headers);
                pipeline.Open();
                pipes.TryAdd(pipelineName, pipeline);
                return pipeline;
            }
            return pipes[pipelineName];
        }

        public IRabbitMqPipeline GetPipeline(Type messageType)
        {
            string pipelineName = nameConvention.GetPipelineName(messageType);
            return GetPipeline(pipelineName);
        }
    }
}