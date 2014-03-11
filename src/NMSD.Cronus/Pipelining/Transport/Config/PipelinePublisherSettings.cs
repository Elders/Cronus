using System;
using System.Reflection;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Pipelining.RabbitMQ.Config;
using NMSD.Cronus.Pipelining.Transport.Strategy;

namespace NMSD.Cronus.Pipelining.Transport.Config
{
    public class PipelinePublisherSettings<T> where T : IPublisher
    {
        public PipelinePublisherSettings()
        {
            TransportSettings = new PipelineTransportSettings<T>();

            bool isCommand = typeof(T).GetGenericArguments()[0] == typeof(ICommand);
            bool isEvent = typeof(T).GetGenericArguments()[0] == typeof(IEvent);
            bool isCommit = typeof(T).GetGenericArguments()[0] == typeof(DomainMessageCommit);

            if (isCommand)
            {
                TransportSettings.PipelineSettings.PipelineNameConvention = new CommandPipelinePerApplication();
            }
            else if (isEvent)
            {
                TransportSettings.PipelineSettings.PipelineNameConvention = new EventPipelinePerApplication();
            }
            else if (isCommit)
            {
                TransportSettings.PipelineSettings.PipelineNameConvention = new EventStorePipelinePerApplication();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public IPipelineTransportSettings<T> TransportSettings;

        public Assembly[] MessagesAssemblies { get; set; }

        public IPublisher Build()
        {
            return null;
        }

    }
}
