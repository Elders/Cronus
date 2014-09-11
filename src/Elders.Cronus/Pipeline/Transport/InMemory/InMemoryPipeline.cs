namespace Elders.Cronus.Pipeline.Transport.InMemory
{
    public class InMemoryPipeline : IPipeline
    {
        private readonly string name;
        private readonly InMemoryPipelineTransport transport;

        public InMemoryPipeline(InMemoryPipelineTransport transport, string name)
        {
            this.transport = transport;
            this.name = name;
        }

        public void Bind(IEndpoint endpoint)
        {
            transport.Bind(this, endpoint);
        }

        public void Push(EndpointMessage message)
        {
            transport.SendMessage(this, message);
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
