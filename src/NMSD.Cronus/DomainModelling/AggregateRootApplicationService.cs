using System;
using NMSD.Cronus.Eventing;
using NMSD.Cronus.Commanding;
using NMSD.Cronus.Messaging;
using System.Runtime.Remoting.Messaging;

namespace NMSD.Cronus.DomainModelling
{
    public interface IAggregateRootApplicationService : IMessageHandler
    {
        IAggregateRepository Repository { get; set; }
    }
    public class AggregateRootApplicationService<AR> : IAggregateRootApplicationService where AR : IAggregateRoot
    {
        public IAggregateRepository Repository { get; set; }
    }
}
