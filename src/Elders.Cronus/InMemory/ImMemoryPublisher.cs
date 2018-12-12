using Elders.Cronus.Multitenancy;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elders.Cronus.InMemory
{
    public abstract class ImMemoryPublisher<T> : Publisher<IMessage> where T : IMessage
    {
        public ImMemoryPublisher(ITenantResolver tenantResolver) : base(tenantResolver) { }

        public override bool Publish(IMessage message, DateTime publishAt, Dictionary<string, string> messageHeaders = null)
        {
            TimeSpan delta = publishAt - DateTime.UtcNow;
            if (delta.TotalSeconds > 10)
                Task.Delay(delta);

            return base.Publish(message, publishAt, messageHeaders);
        }

        public override bool Publish(IMessage message, TimeSpan publishAfter, Dictionary<string, string> messageHeaders = null)
        {
            Task.Delay(publishAfter);

            return base.Publish(message, publishAfter, messageHeaders);
        }
    }
}
