using System;
using System.Runtime.Serialization;
using Elders.Cronus.DomainModelling;
using Elders.Protoreg;

namespace Elders.Cronus.Pipeline
{
    [DataContract(Name = "0c740e60-ea13-4c20-975d-5fd94fc4e920")]
    public class ErrorMessage : IMessage
    {
        ErrorMessage() { }

        public ErrorMessage(object message, Exception exception)
        {
            Messages = message;
            Exception = new ProtoregSerializableException(exception);
        }

        [DataMember(Order = 1)]
        public object Messages { get; private set; }

        [DataMember(Order = 2)]
        public ProtoregSerializableException Exception { get; private set; }
    }
}