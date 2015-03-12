using System;

namespace Elders.Cronus.Pipeline
{
    [Serializable]
    public class EndpointClosedException : Exception
    {
        public EndpointClosedException() { }
        public EndpointClosedException(string message) : base(message) { }
        public EndpointClosedException(string message, Exception inner) : base(message, inner) { }
        protected EndpointClosedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
