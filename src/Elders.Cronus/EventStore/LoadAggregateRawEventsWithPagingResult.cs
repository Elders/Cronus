using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.EventStore
{
    public class LoadAggregateRawEventsWithPagingResult
    {
        public LoadAggregateRawEventsWithPagingResult(IEnumerable<AggregateEventRaw> rawEvents, PagingOptions options)
        {
            if (rawEvents is null)
                rawEvents = Enumerable.Empty<AggregateEventRaw>();

            RawEvents = rawEvents;
            Options = options;
        }

        public IEnumerable<AggregateEventRaw> RawEvents { get; private set; }

        public PagingOptions Options { get; private set; }
    }
}
