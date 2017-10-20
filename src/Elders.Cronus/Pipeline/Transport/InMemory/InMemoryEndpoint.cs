using System;
using System.Collections.Generic;

namespace Elders.Cronus.Pipeline.Transport.InMemory
{
    public class InMemoryEndpoint : IEndpoint
    {
        private readonly InMemoryPipelineTransport transport;
        public InMemoryEndpoint(InMemoryPipelineTransport transport, string name, ICollection<string> watchMessageTypes)
        {
            this.transport = transport;
            Name = name;
            WatchMessageTypes = watchMessageTypes;
        }

        public string Name { get; set; }
        public ICollection<string> WatchMessageTypes { get; set; }

        public CronusMessage Dequeue(TimeSpan timeout)
        {
            return transport.Dequeue(this, timeout);
        }

        public void Acknowledge(CronusMessage message)
        {
        }

        public void Open() { }

        public string RoutingKey { get { return String.Empty; } }

        public override bool Equals(System.Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(InMemoryEndpoint)) return false;
            return Equals((InMemoryEndpoint)obj);
        }

        public virtual bool Equals(IEndpoint other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return 17 ^ Name.GetHashCode();
            }
        }

        public void Dispose()
        {
        }

        public static bool operator ==(InMemoryEndpoint left, InMemoryEndpoint right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left))
                return false;
            else
                return left.Equals(right);
        }

        public static bool operator !=(InMemoryEndpoint a, InMemoryEndpoint b)
        {
            return !(a == b);
        }
    }
}