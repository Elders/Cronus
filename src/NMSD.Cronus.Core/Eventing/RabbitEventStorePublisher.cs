using System;
using System.Linq;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.Messaging;

using NMSD.Protoreg;

namespace NMSD.Cronus.Core.Eventing
{
    public class RabbitEventStorePublisher : RabbitPublisher<MessageCommit>
    {
        public RabbitEventStorePublisher(ProtoregSerializer serialiser)
            : base(serialiser) { }

        protected override bool PublishInternal(MessageCommit message)
        {
            if (String.IsNullOrWhiteSpace(BoundedContext))
            {
                var msg = message.Events.FirstOrDefault();
                if (msg != null)
                {
                    BoundedContext = MessagingHelper.GetBoundedContext(msg.GetType());
                }
            }
            return base.PublishInternal(message);
        }
    }
}