using System;
using Elders.Cronus.EventStore;

namespace Elders.Cronus.IntegrityValidation
{
    public interface IResolver<T> : IComparable<IResolver<T>>
    {
        IntegrityResult<T> Resolve(T eventStream, IValidatorResult validatorResult);

        uint PriorityLevel { get; }
    }

    public class EmptyResolver : IResolver<EventStream>
    {
        public uint PriorityLevel { get { return uint.MaxValue; } }

        public int CompareTo(IResolver<EventStream> other)
        {
            return PriorityLevel.CompareTo(other.PriorityLevel);
        }

        public IntegrityResult<EventStream> Resolve(EventStream eventStream, IValidatorResult validatorResult)
        {
            return new IntegrityResult<EventStream>(eventStream, true);
        }
    }

    public class EmptyResolver<T> : IResolver<T>
    {
        public uint PriorityLevel { get { return uint.MaxValue; } }

        public int CompareTo(IResolver<T> other)
        {
            return PriorityLevel.CompareTo(other.PriorityLevel);
        }

        public IntegrityResult<T> Resolve(T input, IValidatorResult validatorResult)
        {
            return new IntegrityResult<T>(input, true);
        }
    }
}
