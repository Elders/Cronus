using NMSD.Cronus.Core.Eventing;

namespace NMSD.Cronus.Core.Cqrs
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

        public static bool operator ==(AggregateRootState<ID> left, AggregateRootState<ID> right)
        {
            return left.Equals(right);
        }

        public static bool operator >(AggregateRootState<ID> left, AggregateRootState<ID> right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return false;
            if (left == right) return false;
            return left.Id.Equals(right.Id) && left.Version > right.Version;
        }

        public static bool operator !=(AggregateRootState<ID> left, AggregateRootState<ID> right)
        {
            return !(left == right);
        }

        public static bool operator <(AggregateRootState<ID> left, AggregateRootState<ID> right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return false;
            if (left == right) return false;
            return left.Id.Equals(right.Id) && left.Version < right.Version;
        }

        public bool Equals(IAggregateRootState left, IAggregateRootState right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return false;
            if (ReferenceEquals(left, right)) return true;

            return left.Id == right.Id && left.Version == right.Version;
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