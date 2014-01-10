using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.RabbitMQ;

namespace NMSD.Cronus.Transports.RabbitMQ
{
    public class RabbitMqPipelineFactory : IPipelineFactory
    {
        public ConcurrentDictionary<string, RabbitMqPipeline> pipes = new ConcurrentDictionary<string, RabbitMqPipeline>();

        private RabbitMqSession session;

        public RabbitMqPipelineFactory(RabbitMqSession session)
        {
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

        IPipeline IPipelineFactory.GetPipeline(string pipelineName)
        {
            return GetPipeline(pipelineName);
        }

    }
}
