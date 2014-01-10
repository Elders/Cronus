using System;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.EventStoreEngine;
using NMSD.Cronus.Core.Messaging;
using System.Runtime.Remoting.Messaging;
using Cronus.Core.EventStore;

namespace NMSD.Cronus.Core.Cqrs
{
    public interface IAggregateRootApplicationService : IMessageHandler
    {
        IEventStore EventStore { get; set; }
    }
    public class AggregateRootApplicationService<AR> : IAggregateRootApplicationService where AR : IAggregateRoot
    {
       public IEventStore EventStore { get; set; }
    }
}
