using System;

namespace Elders.Cronus.Pipeline
{
    public interface IEndpoint : IEquatable<IEndpoint>, IDisposable
    {
        string Name { get; }

        CronusMessage Dequeue(TimeSpan timeout);

        void Acknowledge(CronusMessage message);
    }
}