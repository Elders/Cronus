using System;

namespace Elders.Cronus.EventStore
{
    public interface IMessageCounter
    {
        void Increment(Type messageType, long incrementWith = 1);
        void Decrement(Type messageType, long decrementWith = 1);
        void Reset(Type messageType);
        long GetCount(Type messageType);
    }
}
