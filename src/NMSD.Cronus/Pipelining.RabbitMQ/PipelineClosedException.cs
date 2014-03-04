using System;

namespace NMSD.Cronus.Transports.RabbitMQ
{
    [Serializable]
    public class PipelineClosedException : Exception
    {
        public PipelineClosedException() { }
        public PipelineClosedException(string message) : base(message) { }
        public PipelineClosedException(string message, Exception inner) : base(message, inner) { }
        protected PipelineClosedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}