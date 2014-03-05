using System;
using System.Reflection;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Transports.Conventions;

namespace NMSD.Cronus.Pipelining.Transport.Config
{
    public class PipelinePublisherSettings<T> where T : IPublisher
    {
        public PipelinePublisherSettings()
        {
            Transport = new PipelineTransportSettings<T>();

            bool isCommand = typeof(T).GetGenericArguments()[0] == typeof(ICommand);
            bool isEvent = typeof(T).GetGenericArguments()[0] == typeof(IEvent);
            bool isCommit = typeof(T).GetGenericArguments()[0] == typeof(DomainMessageCommit);

            if (isCommand)
            {
                Transport.PipelineSettings.PipelineNameConvention = new CommandPipelinePerApplication();
            }
            else if (isEvent)
            {
                Transport.PipelineSettings.PipelineNameConvention = new EventPipelinePerApplication();
            }
            else if (isCommit)
            {
                Transport.PipelineSettings.PipelineNameConvention = new EventStorePipelinePerApplication();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public IPipelineTransportSettings<T> Transport;
        public Assembly MessagesAssembly { get; set; }
    }
}
