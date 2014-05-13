using System;
using System.Reflection;
using Elders.Cronus.Messaging.MessageHandleScope;
using Elders.Cronus.Pipeline.Hosts;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IConsumerSettings
    {
        CronusGlobalSettings GlobalSettings { get; set; }

        Assembly[] MessagesAssemblies { get; set; }

        string BoundedContext { get; set; }

        ScopeFactory ScopeFactory { get; set; }

        int NumberOfWorkers { get; set; }

        void AddRegistration(Type messageType, Type messageHandlerType, Func<Type, Context, object> messageHandlerFactory);
    }

    public static class ConsumerSettingsExtensions
    {
        public static T SetNumberOfWorkers<T>(this T consumer, int workers) where T : IConsumerSettings
        {
            if (workers > 0 && workers < 50)
                consumer.NumberOfWorkers = workers;

            return consumer;
        }
    }
}
