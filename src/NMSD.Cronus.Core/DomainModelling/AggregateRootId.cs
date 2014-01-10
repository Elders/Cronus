using System;
using System.Runtime.Serialization;

namespace NMSD.Cronus.Core.DomainModelling
{
    [DataContract(Name = "b3e2fc15-1996-437d-adfc-64f3b5be3244")]
    public class AggregateRootId : IAggregateRootId
    {
        public AggregateRootId() { }

        public AggregateRootId(Guid idBase)
        {
            this.Id = idBase;
        }

        [DataMember(Order = 1)]
        public Guid Id { get; set; }

        public static implicit operator Guid(AggregateRootId aggregateRootId)
        {
            return aggregateRootId.Id;
        }

        public override bool Equals(System.Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(AggregateRootId)) return false;
            return Equals((AggregateRootId)obj);
        }

        public virtual bool Equals(IAggregateRootId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return 101 ^ Id.GetHashCode() ^ GetType().GetHashCode();
            }
        }

        public static bool operator ==(AggregateRootId a, AggregateRootId b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(AggregateRootId a, AggregateRootId b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return String.Format("AggregateId: {0}", Id);
        }
    }
}