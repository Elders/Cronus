using System;
using System.Runtime.Serialization;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.ProjectionStore
{
    [DataContract(Name = "a556f86c-f4d1-4059-a442-cd6a3b5f0e6d")]
    public class ProjectionCommit
    {
        ProjectionCommit() { }

        public ProjectionCommit(IAggregateRootId aggregateId, int revision, IEvent @event)
        {
            ProjectionId = aggregateId.RawId;
            BoundedContext = aggregateId.GetType().GetBoundedContext().BoundedContextName;
            AggregateRootRevision = revision;
            InternalEvent = @event;
            Timestamp = DateTime.UtcNow.ToFileTimeUtc();
        }

        [DataMember(Order = 1)]
        public byte[] ProjectionId { get; private set; }

        [DataMember(Order = 2)]
        public string BoundedContext { get; private set; }

        [DataMember(Order = 3)]
        public int AggregateRootRevision { get; private set; }

        [DataMember(Order = 4)]
        private object InternalEvent { get; set; }

        [DataMember(Order = 5)]
        public long Timestamp { get; private set; }

        public IEvent Event { get { return InternalEvent as IEvent; } }
    }
}