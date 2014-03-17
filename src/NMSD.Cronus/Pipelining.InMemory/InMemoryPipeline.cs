using System;
using NMSD.Cronus.Pipelining;
using NMSD.Cronus.Transports;

namespace NMSD.Cronus.Transport.InMemory
{
    public class InMemoryPipeline : IPipeline
    {
        private readonly string name;

        public InMemoryPipeline(string name)
        {
            this.name = name;
        }

        public void Bind(IEndpoint endpoint)
        {
            InMemoryQueue.Current.Bind(this, endpoint);
        }

        public void Push(EndpointMessage message)
        {
            InMemoryQueue.Current.SendMessage(this, message);
        }

        public string Name
        {
            get { return name; }
        }

        public override bool Equals(System.Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(InMemoryPipeline)) return false;
            return Equals((InMemoryPipeline)obj);
        }

        public virtual bool Equals(IPipeline other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return 11 ^ Name.GetHashCode();
            }
        }

        public static bool operator ==(InMemoryPipeline left, InMemoryPipeline right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left))
                return false;
            else
                return left.Equals(right);
        }

        public static bool operator !=(InMemoryPipeline a, InMemoryPipeline b)
        {
            return !(a == b);
        }

    }
}
