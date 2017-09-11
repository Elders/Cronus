using System;
using System.Collections.Generic;
using Elders.Cronus.Serializer;

namespace Elders.Cronus.Pipeline
{
    public interface IEndpoint : IEquatable<IEndpoint>
    {
        string Name { get; }

        void OnMessage(Action<CronusMessage> action);

        void Start();

        void Stop();
    }
}