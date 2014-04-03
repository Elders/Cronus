using System;
using System.Collections.Generic;
using System.Reflection;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Messaging.MessageHandleScope;
using Elders.Cronus.Pipeline.Hosts;

namespace Elders.Cronus.Pipeline.Config
{
    public abstract class ConsumerSettings<TContract, TTransport> : IConsumerSettings<TTransport>
        where TContract : IMessage
        where TTransport : TransportSettings
    {
        protected Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>> registrations = new Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>>();

        public ConsumerSettings()
        {
            ScopeFactory = new ScopeFactory();
            NumberOfWorkers = 1;
            ConsumerBatchSize = 1;
        }

        public string BoundedContext { get; set; }

        public IEndpointConsumerErrorStrategy ErrorStrategy { get; set; }

        public IEndpointConsumerSuccessStrategy SuccessStrategy { get; set; }

        public CronusGlobalSettings GlobalSettings { get; set; }

        public int ConsumerBatchSize { get; set; }

        public Assembly[] MessagesAssemblies { get; set; }

        public int NumberOfWorkers { get; set; }

        public ScopeFactory ScopeFactory { get; set; }

        public TTransport Transport { get; set; }

        public void AddRegistration(Type messageType, Type messageHandlerType, Func<Type, Context, object> messageHandlerFactory)
        {
            if (!registrations.ContainsKey(messageType))
            {
                registrations.Add(messageType, new List<Tuple<Type, Func<Type, Context, object>>>());
            }
            registrations[messageType].Add(new Tuple<Type, Func<Type, Context, object>>(messageHandlerType, messageHandlerFactory));
        }

        public abstract IEndpointConsumable Build();

        public void UseTransport<T>(Action<T> configure = null) where T : TTransport
        {
            T transport = Activator.CreateInstance<T>();
            if (configure != null)
                configure(transport);
            Transport = transport;
        }

        protected abstract IEndpointConsumable BuildConsumer();

    }
}