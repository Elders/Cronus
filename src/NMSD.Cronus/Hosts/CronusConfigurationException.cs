using System;

namespace NMSD.Cronus.Hosts
{
    [Serializable]
    public class CronusConfigurationException : Exception
    {
        public CronusConfigurationException() { }
        public CronusConfigurationException(string message) : base(message) { }
        public CronusConfigurationException(string message, Exception inner) : base(message, inner) { }
        protected CronusConfigurationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}