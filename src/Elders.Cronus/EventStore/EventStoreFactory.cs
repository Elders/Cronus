using Elders.Cronus.EventStore.Index;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

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

    public IEventStore GetEventStore()
    {
        return new CronusEventStore(eventStore, logger);
    }
}

public sealed class MIssingEventStoreFactory : IEventStoreFactory
{
    public IEventStore GetEventStore()
    {
        throw new System.NotImplementedException("The EventStore is not configured. You need to do the following steps: 1. Set Cronus:ApplicationServicesEnabled = true. 2. Install nuget package Cronus.Persistence.Cassandra.");
    }
}
