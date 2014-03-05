using System;
using System.Collections.Generic;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Hosting;
using NMSD.Cronus.Messaging.MessageHandleScope;
using NMSD.Cronus.Transports.Conventions;

namespace NMSD.Cronus.Pipelining.Transport.Config
{
    public class PipelineConsumerSettings<T> where T : IEndpointConsumer<IMessage>
    {
        public PipelineConsumerSettings()
        {
            NumberOfWorkers = 1;
            Transport = new PipelineTransportSettings<T>();

            bool isCommand = typeof(T).GetGenericArguments()[0] == typeof(ICommand);
            bool isEvent = typeof(T).GetGenericArguments()[0] == typeof(IEvent);

            if (isCommand)
            {
                Transport.PipelineSettings.PipelineNameConvention = new CommandPipelinePerApplication();
                Transport.PipelineSettings.EndpointNameConvention = new CommandHandlerEndpointPerBoundedContext(Transport.PipelineSettings.PipelineNameConvention);
            }
            else if (isEvent)
            {
                Transport.PipelineSettings.PipelineNameConvention = new EventPipelinePerApplication();
                Transport.PipelineSettings.EndpointNameConvention = new EventHandlerEndpointPerHandler(Transport.PipelineSettings.PipelineNameConvention);
            }
            else
            {
                throw new NotImplementedException();
            }
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