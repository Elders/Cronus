using System;
using System.Collections.Generic;
using System.Text;

namespace Elders.Cronus.Projections
{
    public static class CronusMessageExtensions
    {
        public static EventOrigin GetEventOrigin(this CronusMessage message)
        {
            return new EventOrigin(GetRootId(message), GetRevision(message), GetRootEventPosition(message), GetRootEventTimestamp(message));
        }

        public static int GetRevision(this CronusMessage message)
        {
            var revision = 0;
            var value = string.Empty;
            if (message.Headers.TryGetValue(MessageHeader.AggregateRootRevision, out value) && int.TryParse(value, out revision))
                return revision;
            return 0;
        }

        public static int GetRootEventPosition(this CronusMessage message)
        {
            var revision = 0;
            var value = string.Empty;
            if (message.Headers.TryGetValue(MessageHeader.AggregateRootEventPosition, out value) && int.TryParse(value, out revision))
                return revision;
            return 0;
        }

        public static long GetRootEventTimestamp(this CronusMessage message)
        {
            long timestamp = 0;
            var value = string.Empty;
            if (message.Headers.TryGetValue(MessageHeader.PublishTimestamp, out value) && long.TryParse(value, out timestamp))
                return timestamp;
            return 0;
        }

        public static string GetRootId(this CronusMessage message)
        {
            var aggregateRootId = string.Empty;
            if (message.Headers.TryGetValue(MessageHeader.AggregateRootId, out aggregateRootId))
                return aggregateRootId;

            throw new ArgumentException("Cronus message does not contain a valid AggregateRootId");
        }
    }
}
