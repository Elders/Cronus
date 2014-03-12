using System;
using System.Collections.Generic;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Hosting;
using NMSD.Protoreg;

namespace NMSD.Cronus.Pipelining.Host.Config
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
