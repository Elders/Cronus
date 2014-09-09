using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.DomainModeling
{
    [DataContract(Name = "71a0dc2e-1d59-4818-af05-222b334fffbe")]
    public class TransportMessage
    {
        TransportMessage() { }

        public TransportMessage(TransportMessage transportMessage)
        {
            Id = transportMessage.Id;
            Age = transportMessage.Age + 1;
            Payload = transportMessage.Payload;
        }

        public TransportMessage(Guid id, IMessage message)
        {
            Id = id;
            Age = 1;
            Payload = message;
        }

        public TransportMessage(IMessage message)
        {
            Id = Guid.NewGuid();
            Age = 1;
            Payload = message;
        }

        [DataMember(Order = 1)]
        public Guid Id { get; private set; }

        [DataMember(Order = 2)]
        public int Age { get; private set; }

        [DataMember(Order = 3)]
        public object Payload { get; private set; }
    }
}