using System;
using System.Collections.Generic;
using NMSD.Cronus.Messaging.MessageHandleScope;
using NMSD.Cronus.Transports.Conventions;

namespace NMSD.Cronus.Pipelining.Transport.Config
{
    public class PipelineEventStoreConsumerSettings<T> where T : EndpointEventStoreConsumer
    {
        public PipelineEventStoreConsumerSettings()
        {
            NumberOfWorkers = 1;
            Transport = new PipelineTransportSettings<T>();
            Transport.PipelineSettings.PipelineNameConvention = new EventStorePipelinePerApplication();
            Transport.PipelineSettings.EndpointNameConvention = new EventStoreEndpointPerBoundedContext(Transport.PipelineSettings.PipelineNameConvention);
        }

        public int NumberOfWorkers { get; set; }

        public Type AssemblyEventsWhichWillBeIntercepted { get; set; }

        public IPipelineTransportSettings<T> Transport;
    }
}
