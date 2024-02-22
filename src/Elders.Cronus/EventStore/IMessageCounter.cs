using System;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore;

public interface IMessageCounter
{
    Task IncrementAsync(Type messageType, long incrementWith = 1);
    Task DecrementAsync(Type messageType, long decrementWith = 1);
    Task ResetAsync(Type messageType);
    Task<long> GetCountAsync(Type messageType);
}
