﻿using Microsoft.Extensions.Logging;

namespace Elders.Cronus.EventStore;

public interface IEventStoreFactory
{
    IEventStore GetEventStore();
}

public sealed class EventStoreFactory : IEventStoreFactory
{
    private readonly IEventStore eventStore;
    private readonly ILogger<CronusEventStore> logger;

    public EventStoreFactory(IEventStore eventStore, ILogger<CronusEventStore> logger)
    {
        this.eventStore = eventStore;
        this.logger = logger;
    }

    public IEventStore GetEventStore() => new CronusEventStore(eventStore, logger);
}
