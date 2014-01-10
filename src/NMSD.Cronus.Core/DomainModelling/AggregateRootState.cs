using NMSD.Cronus.Core.Eventing;

namespace NMSD.Cronus.Core.DomainModelling
{
    public abstract class AggregateRootState<ID> : IAggregateRootState
        where ID : IAggregateRootId
    {
        IAggregateRootId IAggregateRootState.Id { get { return Id; } }

        public abstract ID Id { get; set; }

        public abstract int Version { get; set; }

        public void Apply(IEvent @event)
        {
            var state = (dynamic)this;
            state.When((dynamic)@event);
        }

        public static bool operator ==(AggregateRootState<ID> x, AggregateRootState<ID> y)
        {
            return x.Equals(y);
        }

        public static bool operator >(AggregateRootState<ID> x, AggregateRootState<ID> y)
        {
            if (ReferenceEquals(null, x) && ReferenceEquals(null, y)) return false;
            if (x == y) return false;
            return x.Id.Equals(y.Id) && x.Version > y.Version;
        }

        public static bool operator !=(AggregateRootState<ID> x, AggregateRootState<ID> y)
        {
            return !(x == y);
        }

        public static bool operator <(AggregateRootState<ID> x, AggregateRootState<ID> y)
        {
            if (ReferenceEquals(null, x) && ReferenceEquals(null, y)) return false;
            if (x == y) return false;
            return x.Id.Equals(y.Id) && x.Version < y.Version;
        }

        public bool Equals(IAggregateRootState x, IAggregateRootState y)
        {
            if (ReferenceEquals(null, x) && ReferenceEquals(null, y)) return false;
            if (ReferenceEquals(x, y)) return true;

            return x.Id == y.Id && x.Version == y.Version;
        }

        public bool Equals(IAggregateRootState other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Id.Equals(other.Id) && Version == other.Version;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var casted = obj as AggregateRootState<ID>;
            if (casted != null)
                return Equals(casted);
            else
                return false;
        }

        public int GetHashCode(IAggregateRootState obj)
        {
            return obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = Id.GetHashCode();
                result = (result * 101) ^ Version.GetHashCode();
                return result;
            }
        }

    }
}