using System.Collections.Generic;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Protoreg;

namespace Elders.Cronus.Pipeline.Hosts
{
    public class CronusGlobalSettings
    {
        public CronusGlobalSettings()
        {
            AggregateRepositories = new Dictionary<string, IAggregateRepository>();
            EventStorePlayers = new Dictionary<string, IEventStorePlayer>();
            EventStorePersisters = new Dictionary<string, IEventStorePersister>();
            EventStoreHandlers = new Dictionary<string, IMessageProcessor<DomainMessageCommit>>();
        }

        public Dictionary<string, IAggregateRepository> AggregateRepositories { get; set; }
        public Dictionary<string, IEventStorePlayer> EventStorePlayers { get; set; }
        public Dictionary<string, IEventStorePersister> EventStorePersisters { get; set; }
        public Dictionary<string, IMessageProcessor<DomainMessageCommit>> EventStoreHandlers { get; set; }

        public Dictionary<IEndpointConsumable, int> Consumers = new Dictionary<IEndpointConsumable, int>();

        public ProtoRegistration Protoreg { get; set; }

        public ProtoregSerializer Serializer { get; set; }

        public IPublisher<ICommand> CommandPublisher { get; set; }

        public IPublisher<IEvent> EventPublisher { get; set; }

        public IPublisher<DomainMessageCommit> EventStorePublisher { get; set; }
    }
}
