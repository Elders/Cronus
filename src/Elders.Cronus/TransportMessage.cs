using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Elders.Cronus.DomainModeling;
using Elders.Protoreg;

namespace Elders.Cronus
{
    [DataContract(Name = "71a0dc2e-1d59-4818-af05-222b334fffbe")]
    public class TransportMessage : IEquatable<TransportMessage>
    {
        TransportMessage()
        {
            InternalErrors = new List<object>();
        }

        public TransportMessage(TransportMessage transportMessage, FeedError error = null)
        {
            Id = transportMessage.Id;
            Age = transportMessage.Age;
            Payload = transportMessage.Payload;
            InternalErrors = new List<object>(transportMessage.InternalErrors);
            if (error != null)
                InternalErrors.Add(error);
        }

        public TransportMessage(Guid id, Message message)
        {
            Id = id;
            Age = 1;
            Payload = message;
            InternalErrors = new List<object>();
        }

        public TransportMessage(Message message)
        {
            Id = Guid.NewGuid();
            Age = 1;
            Payload = message;
            InternalErrors = new List<object>();
        }

        [DataMember(Order = 1)]
        public Guid Id { get; private set; }

        [DataMember(Order = 2)]
        public int Age { get; set; }

        [Obsolete("Do not use this. Will be removed after 2015-07-01")]
        [DataMember(Order = 3)]
        public object Obsolete { get; private set; }

        [DataMember(Order = 4)]
        private List<object> InternalErrors { get; set; }
        public List<FeedError> Errors { get { return InternalErrors.Cast<FeedError>().ToList(); } }

        [DataMember(Order = 5)]
        public Message Payload { get; private set; }

        public override bool Equals(System.Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!typeof(TransportMessage).IsAssignableFrom(obj.GetType())) return false;
            return Equals((TransportMessage)obj);
        }

        public virtual bool Equals(TransportMessage other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.Id == other.Id && this.Age == other.Age;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return 103 ^ this.Id.GetHashCode() ^ this.Age.GetHashCode();
            }
        }

        public static bool operator ==(TransportMessage left, TransportMessage right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left))
                return false;
            else
                return left.Equals(right);
        }

        public static bool operator !=(TransportMessage a, TransportMessage b)
        {
            return !(a == b);
        }
    }

    [DataContract(Name = "020dfc56-c698-47d2-b9c6-0b36af7637a0")]
    public class FeedError
    {
        [DataMember(Order = 1)]
        public ErrorOrigin Origin { get; set; }

        [DataMember(Order = 2)]
        public ProtoregSerializableException Error { get; set; }
    }

    [DataContract(Name = "a4ede1c9-38cb-4f45-a201-e79133bb8b20")]
    public class ErrorOrigin
    {
        ErrorOrigin() { }

        public ErrorOrigin(string id, string type)
        {
            Id = id;
            Type = type;
        }

        [DataMember(Order = 1)]
        public string Id { get; set; }

        [DataMember(Order = 2)]
        public string Type { get; set; }
    }

    internal class ErrorOriginType
    {
        private readonly string errorOriginType;

        public static ErrorOriginType MessageHandler = new ErrorOriginType("handler");
        public static ErrorOriginType UnitOfWork = new ErrorOriginType("uow");

        private ErrorOriginType(string errorOriginType)
        {
            this.errorOriginType = errorOriginType;
        }

        public override string ToString()
        {
            return errorOriginType;
        }

        public static implicit operator string (ErrorOriginType scopeType)
        {
            return scopeType.ToString();
        }
    }
}