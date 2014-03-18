using System;
using System.Reflection;
using NMSD.Cronus.Messaging.MessageHandleScope;
using NMSD.Cronus.Pipeline.Hosts;

namespace NMSD.Cronus.Pipeline.Config
{
    public interface IConsumerSettings<TTransport> where TTransport : TransportSettings
    {
        CronusGlobalSettings GlobalSettings { get; set; }

        Assembly[] MessagesAssemblies { get; set; }

        TTransport Transport { get; set; }

        IEndpointConsumable Build();

        string BoundedContext { get; set; }

        ScopeFactory ScopeFactory { get; set; }

        int NumberOfWorkers { get; set; }

        void UseTransport<T>(Action<T> configure) where T : TTransport;

        void AddRegistration(Type messageType, Type messageHandlerType, Func<Type, Context, object> messageHandlerFactory);
    }
}
