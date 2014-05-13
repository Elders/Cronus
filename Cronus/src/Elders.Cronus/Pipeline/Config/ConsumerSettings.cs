using System;
using System.Collections.Generic;
using System.Reflection;
using Elders.Cronus.Messaging.MessageHandleScope;
using Elders.Cronus.Pipeline.Hosts;

namespace Elders.Cronus.Pipeline.Config
{
    public abstract class ConsumerSettings : IConsumerSettings
    {
        protected Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>> registrations = new Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>>();

        public ConsumerSettings()
        {
            ScopeFactory = new ScopeFactory();
            this.SetNumberOfWorkers(1);
            ConsumerBatchSize = 1;
        }

        public string BoundedContext { get; set; }

        public CronusGlobalSettings GlobalSettings { get; set; }

        public int ConsumerBatchSize { get; set; }

        public Assembly[] MessagesAssemblies { get; set; }

        int IConsumerSettings.NumberOfWorkers { get; set; }

        public ScopeFactory ScopeFactory { get; set; }

        public void AddRegistration(Type messageType, Type messageHandlerType, Func<Type, Context, object> messageHandlerFactory)
        {
            if (!registrations.ContainsKey(messageType))
            {
                registrations.Add(messageType, new List<Tuple<Type, Func<Type, Context, object>>>());
            }
            registrations[messageType].Add(new Tuple<Type, Func<Type, Context, object>>(messageHandlerType, messageHandlerFactory));
        }
    }
}