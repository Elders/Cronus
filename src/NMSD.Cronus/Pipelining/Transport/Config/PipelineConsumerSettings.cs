using System;
using System.Collections.Generic;
using System.Reflection;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging.MessageHandleScope;
using NMSD.Cronus.Pipelining.Transport.Strategy;

namespace NMSD.Cronus.Pipelining.Transport.Config
{
    public class PipelineConsumerSettings<T> where T : IEndpointConsumer<IMessage>
    {
        public PipelineConsumerSettings()
        {
            NumberOfWorkers = 1;
            ScopeFactory = new ScopeFactory();
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
                Transport.PipelineSettings.EndpointNameConvention = new EventHandlerEndpointPerBoundedContext(Transport.PipelineSettings.PipelineNameConvention);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public int NumberOfWorkers { get; set; }

        public ScopeFactory ScopeFactory { get; set; }

        public Assembly[] MessagesAssemblies { get; set; }

        public IPipelineTransportSettings<T> Transport;

        public void AddRegistration(Type messageType, Type messageHandlerType, Func<Type, Context, object> messageHandlerFactory)
        {
            if (!Registrations.ContainsKey(messageType))
            {
                Registrations.Add(messageType, new List<Tuple<Type, Func<Type, Context, object>>>());
            }
            Registrations[messageType].Add(new Tuple<Type, Func<Type, Context, object>>(messageHandlerType, messageHandlerFactory));
        }

        public Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>> Registrations = new Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>>();
    }
}