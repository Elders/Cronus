using Microsoft.Extensions.Logging;

namespace Elders.Cronus.EventStore;

public sealed class EventStoreFactory
{
    private readonly IEventStore eventStore;
    private readonly ILogger<CronusEventStore> logger;

    public EventStoreFactory(IEventStore eventStore, ILogger<CronusEventStore> logger)
    {
        this.eventStore = eventStore;
        this.logger = logger;
    }

    public IEventStore GetEventStore()
    {
        return new CronusEventStore(eventStore, logger);
    }
}
