using System;
using System.Runtime.Serialization;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Serializer;

namespace Elders.Cronus.Pipeline
{
    [DataContract(Name = "0c740e60-ea13-4c20-975d-5fd94fc4e920")]
    public class ErrorMessage : IMessage
    {
        ErrorMessage() { }

        public ErrorMessage(object message, Exception exception)
        {
            System.Diagnostics.Trace.WriteLine("We should move Elders.Protoreg.ProtoregSerializableException from protoreg to Cronus");
            Messages = message;
            Exception = new Elders.Protoreg.ProtoregSerializableException(exception);
        }

        [DataMember(Order = 1)]
        public object Messages { get; private set; }

        [DataMember(Order = 2)]
        public Elders.Protoreg.ProtoregSerializableException Exception { get; private set; }
    }
}