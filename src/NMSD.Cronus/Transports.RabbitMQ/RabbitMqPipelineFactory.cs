using System;
using System.Collections.Concurrent;

namespace NMSD.Cronus.Transports.RabbitMQ
{
    public class RabbitMqPipelineFactory : IPipelineFactory
    {
        private readonly IPipelineNameConvention nameConvention;

        public ConcurrentDictionary<string, RabbitMqPipeline> pipes = new ConcurrentDictionary<string, RabbitMqPipeline>();

        private RabbitMqSession session;

        public RabbitMqPipelineFactory(RabbitMqSession session, IPipelineNameConvention nameConvention)
        {
            this.nameConvention = nameConvention;
            this.session = session;
        }

        public RabbitMqPipeline GetPipeline(string pipelineName)
        {
            if (!pipes.ContainsKey(pipelineName))
            {
                var pipeline = new RabbitMqPipeline(pipelineName, session, RabbitMqPipeline.PipelineType.Headers);
                pipeline.Open();
                pipes.TryAdd(pipelineName, pipeline);
                return pipeline;
            }
            return pipes[pipelineName];
        }

        public RabbitMqPipeline GetPipeline(Type messageType)
        {
            string pipelineName = nameConvention.GetPipelineName(messageType);
            return GetPipeline(pipelineName);
        }

        IPipeline IPipelineFactory.GetPipeline(string pipelineName)
        {
            return GetPipeline(pipelineName);
        }

        IPipeline IPipelineFactory.GetPipeline(Type messageType)
        {
            return GetPipeline(messageType);
        }
    }
}