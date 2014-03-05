using System;
using System.Collections.Generic;
using System.IO;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Transports;
using NMSD.Protoreg;

namespace NMSD.Cronus.Pipelining
{
    public class EndpointEventStoreConsumer : IEndpointConsumer<DomainMessageCommit>
    {
        private readonly Type assemblyContainingEventsByEventType;
        private readonly IPublisher commandPublisher;
        private readonly IPublisher eventPublisher;
        private readonly IEventStore eventStore;
        private readonly ProtoregSerializer serialiser;

        public EndpointEventStoreConsumer(IEventStore eventStore, IPublisher eventPublisher, IPublisher commandPublisher, Type assemblyContainingEventsByEventType, ProtoregSerializer serialiser)
        {
            this.assemblyContainingEventsByEventType = assemblyContainingEventsByEventType;
            this.commandPublisher = commandPublisher;
            this.eventPublisher = eventPublisher;
            this.serialiser = serialiser;
            this.eventStore = eventStore;
        }

        public bool Consume(IEndpoint endpoint)
        {
            eventStore.UseStream(() =>
            {
                EndpointMessage rawMessage;
                DomainMessageCommit commit = null;
                if (endpoint.BlockDequeue(30, out rawMessage))
                {
                    using (var stream = new MemoryStream(rawMessage.Body))
                    {
                        commit = serialiser.Deserialize(stream) as DomainMessageCommit;
                    }
                }
                return commit;
            },
            (eventStream, commit) => commit == null || eventStream.Events.Count == 100,
            events =>
            {
                foreach (var @event in events)
                {
                    eventPublisher.Publish(@event);
                }
                endpoint.AcknowledgeAll();
            },
            eventStream => false,
            commit =>
            {
                commandPublisher.Publish(commit.Command);
                endpoint.AcknowledgeAll();
            });
            return true;
        }

        public IEnumerable<Type> GetRegisteredHandlers
        {
            get { yield return assemblyContainingEventsByEventType; }
        }
    }
}
