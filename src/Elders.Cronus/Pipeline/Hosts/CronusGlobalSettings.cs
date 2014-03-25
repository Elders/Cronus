using System.Collections.Generic;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Protoreg;

namespace Elders.Cronus.Pipeline.Hosts
{
    public class CronusGlobalSettings
    {
        public Dictionary<IEndpointConsumable, int> Consumers = new Dictionary<IEndpointConsumable, int>();

        public List<IEventStore> EventStores = new List<IEventStore>();

        public ProtoRegistration Protoreg { get; set; }

        public ProtoregSerializer Serializer { get; set; }

        public IPublisher<ICommand> CommandPublisher { get; set; }

        public IPublisher<IEvent> EventPublisher { get; set; }

        public IPublisher<DomainMessageCommit> EventStorePublisher { get; set; }

    }
}
