using System;
using System.Reflection;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Pipeline.Hosts;

namespace Elders.Cronus.Pipeline.Config
{
    public abstract class PublisherSettings<TContract, TTransport>
        where TContract : IMessage
        where TTransport : TransportSettings
    {
        public CronusGlobalSettings GlobalSettings { get; set; }

        public Assembly[] MessagesAssemblies { get; set; }

        public TTransport Transport { get; set; }

        public abstract IPublisher<TContract> Build();

        public PublisherSettings<TContract, TTransport> UseTransport<T>(Action<T> configure = null) where T : TTransport
        {
            T transport = Activator.CreateInstance<T>();
            if (configure != null)
                configure(transport);
            Transport = transport;

            return this;
        }

        protected abstract IPublisher<TContract> BuildPublisher();

    }
}
