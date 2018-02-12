using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Elders.Cronus
{
    [DataContract(Name = "71a0dc2e-1d59-4818-af05-222b334fffbe")]
    public class CronusMessage : IEquatable<CronusMessage>
    {
        CronusMessage()
        {
            Headers = new Dictionary<string, string>();
        }

        public CronusMessage(IMessage message, IDictionary<string, string> headers) : this()
        {
            Id = Guid.NewGuid();
            Payload = message;
            Headers = headers;
        }

        [DataMember(Order = 1)]
        public Guid Id { get; private set; }

        [DataMember(Order = 2)]
        public IMessage Payload { get; private set; }

        [DataMember(Order = 3)]
        public IDictionary<string, string> Headers { get; private set; }

        public string MessageId { get { return GetHeader(MessageHeader.MessageId); } }

        public string CausationId { get { return GetHeader(MessageHeader.CausationId); } }

        public string CorelationId { get { return GetHeader(MessageHeader.CorelationId); } }

        string GetHeader(string key)
        {
            string value;
            if (Headers.TryGetValue(key, out value) == false && MessageHeader.MessageId.Equals(key) == false)
                value = $"Missing {key} for {GetHeader(MessageHeader.MessageId)}";

            return value;
        }

        public override bool Equals(System.Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!typeof(CronusMessage).IsAssignableFrom(obj.GetType())) return false;
            return Equals((CronusMessage)obj);
        }

        public virtual bool Equals(CronusMessage other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.Id == other.Id;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return 103 ^ this.Id.GetHashCode();
            }
        }

        public static bool operator ==(CronusMessage left, CronusMessage right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left))
                return false;
            else
                return left.Equals(right);
        }

        public static bool operator !=(CronusMessage a, CronusMessage b)
        {
            return !(a == b);
        }
    }
}
