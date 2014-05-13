using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Elders.Cronus.DomainModelling;
using Elders.Protoreg;

namespace Elders.Cronus.Pipeline
{
    [DataContract(Name = "0c740e60-ea13-4c20-975d-5fd94fc4e920")]
    public class ErrorMessage : IMessage
    {
        ErrorMessage() { }

        public ErrorMessage(List<object> messages, Exception exception)
        {
            Messages = messages;
            Exception = new ProtoregSerializableException(exception);
        }

        [DataMember(Order = 1)]
        public List<object> Messages { get; private set; }

        [DataMember(Order = 2)]
        public ProtoregSerializableException Exception { get; private set; }
    }
}