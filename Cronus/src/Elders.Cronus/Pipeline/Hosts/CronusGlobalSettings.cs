using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Protoreg;

namespace Elders.Cronus.Pipeline.Hosts
{
    public class CronusConfiguration
    {
        public CronusConfiguration()
        {
            EventStores = new Dictionary<string, IEventStore>();
            Consumers = new List<IEndpointConsumable>();
        }


        public Dictionary<string, IEventStore> EventStores { get; set; }

        public List<IEndpointConsumable> Consumers { get; set; }

        public ProtoRegistration Protoreg { get; set; }

        public ProtoregSerializer Serializer { get; set; }

        public IPublisher<ICommand> CommandPublisher { get; set; }

        public IPublisher<IEvent> EventPublisher { get; set; }

        public IPublisher<DomainMessageCommit> EventStorePublisher { get; set; }
    }
}
