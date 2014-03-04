using System;
using System.Collections.Generic;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Hosting;
using NMSD.Cronus.Messaging.MessageHandleScope;
using NMSD.Cronus.Transports.Conventions;

namespace NMSD.Cronus.Pipelining.Transport.Config
{
    public class PipelineConsumerSettings<T> where T : IStartableConsumer<IMessage>
    {
        public PipelineConsumerSettings()
        {
            NumberOfWorkers = 1;
            Transport = new PipelineTransportSettings<T>();
            Transport.PipelineSettings.PipelineNameConvention = new CommandPipelinePerApplication();
            Transport.PipelineSettings.EndpointNameConvention = new CommandHandlerEndpointPerBoundedContext(Transport.PipelineSettings.PipelineNameConvention);
        }

        public int NumberOfWorkers { get; set; }

        public ScopeFactory ScopeFactory { get; set; }

        public IPipelineTransportSettings<T> Transport;

        public void AddRegistration(Type messageType, Type messageHandlerType, Func<Type, Context, object> messageHandlerFactory)
        {
            Registrations.Add(messageType, new Tuple<Type, Func<Type, Context, object>>(messageHandlerType, messageHandlerFactory));
        }

        public Dictionary<Type, Tuple<Type, Func<Type, Context, object>>> Registrations = new Dictionary<Type, Tuple<Type, Func<Type, Context, object>>>();
    }
}