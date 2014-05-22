using System;

namespace Elders.Cronus.DomainModelling
{
    public interface IMessage
    {

    }

    public class TransportMessage
    {
        public Guid Id { get; set; }

        public int Age { get; set; }

        public object Payload { get; set; }
    }
}