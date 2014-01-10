using System;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Messaging;
using System.Runtime.Remoting.Messaging;

namespace NMSD.Cronus.Core.DomainModelling
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
