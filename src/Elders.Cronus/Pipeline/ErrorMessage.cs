using System;
using System.Runtime.Serialization;
using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Pipeline
{
    [DataContract(Name = "0c740e60-ea13-4c20-975d-5fd94fc4e920")]
    public class ErrorMessage : IMessage
    {
        ErrorMessage() { }

        public ErrorMessage(object message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }

        [DataMember(Order = 1)]
        public object Message { get; private set; }


        public Exception Exception { get; private set; }
    }
}