using System;
using System.Linq;
using System.Reflection;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Pipelining.Hosts.Config;
using NMSD.Cronus.Pipelining.Strategy;

namespace NMSD.Cronus.Pipelining.Config
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

    public abstract class PipelinePublisherSetting<TContract> : PublisherSettings<TContract, PipelineTransportSettings> where TContract : IMessage
    {
        public PipelinePublisherSetting()
        {
            PipelineSettings = new PipelineSettings();
        }

        public PipelineSettings PipelineSettings { get; set; }

        public override IPublisher<TContract> Build()
        {
            Transport.Build(PipelineSettings);
            if (MessagesAssemblies != null)
                MessagesAssemblies.ToList().ForEach(ass => GlobalSettings.Protoreg.RegisterAssembly(ass));
            return BuildPublisher();
        }

    }

    public class PipelineCommandPublisherSettings : PipelinePublisherSetting<ICommand>
    {
        public PipelineCommandPublisherSettings()
        {
            PipelineSettings.PipelineNameConvention = new CommandPipelinePerApplication();
        }

        protected override IPublisher<ICommand> BuildPublisher()
        {
            return new PipelinePublisher<ICommand>(Transport.PipelineFactory, GlobalSettings.Serializer);
        }
    }

    public class PipelineEventPublisherSettings : PipelinePublisherSetting<IEvent>
    {
        public PipelineEventPublisherSettings()
        {
            PipelineSettings.PipelineNameConvention = new EventPipelinePerApplication();
        }

        protected override IPublisher<IEvent> BuildPublisher()
        {
            return new PipelinePublisher<IEvent>(Transport.PipelineFactory, GlobalSettings.Serializer);
        }
    }

    public class PipelineEventStorePublisherSettings : PipelinePublisherSetting<DomainMessageCommit>
    {
        public PipelineEventStorePublisherSettings()
        {
            PipelineSettings.PipelineNameConvention = new EventStorePipelinePerApplication();
        }

        protected override IPublisher<DomainMessageCommit> BuildPublisher()
        {
            return new EventStorePublisher(Transport.PipelineFactory, GlobalSettings.Serializer);
        }
    }
}
