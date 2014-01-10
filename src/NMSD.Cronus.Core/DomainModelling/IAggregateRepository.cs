using System.Collections.Generic;
using System.Data.SqlClient;
using NMSD.Cronus.Core.Eventing;

namespace NMSD.Cronus.Core.DomainModelling
{
    /// <summary>
    /// Indicates the ability to store and retreive a stream of events.
    /// </summary>
    /// <remarks>
    /// Instances of this class must be designed to be multi-thread safe such that they can be shared between threads.
    /// </remarks>
    public interface IAggregateRepository
    {
        AR Load<AR>(IAggregateRootId aggregateId) where AR : IAggregateRoot;

        void Save(IAggregateRoot aggregateRoot);

       
    }
 
}
