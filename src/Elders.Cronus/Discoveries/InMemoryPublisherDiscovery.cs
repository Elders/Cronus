using System.Collections.Generic;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.AtomicAction.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries;

public class InMemoryDiscovery : DiscoveryBase<IPublisher<IMessage>>
{
    protected override DiscoveryResult<IPublisher<IMessage>> DiscoverFromAssemblies(DiscoveryContext context)
    {
        return new DiscoveryResult<IPublisher<IMessage>>(GetModels());
    }

    IEnumerable<DiscoveredModel> GetModels()
    {
        //yield return new DiscoveredModel(typeof(IConsumer<>), typeof(EmptyConsumer<>), ServiceLifetime.Singleton);

        yield return new DiscoveredModel(typeof(IAggregateRootAtomicAction), typeof(InMemoryAggregateRootAtomicAction), ServiceLifetime.Transient);

        // The order matters here. If you reorder the registrations bellow then you know what you are doing.
        yield return new DiscoveredModel(typeof(DelegatingPublishHandler), typeof(LoggingPublishHandler), ServiceLifetime.Transient) { CanAddMultiple = true };
        yield return new DiscoveredModel(typeof(DelegatingPublishHandler), typeof(CronusHeadersPublishHandler), ServiceLifetime.Transient) { CanAddMultiple = true };
        yield return new DiscoveredModel(typeof(DelegatingPublishHandler), typeof(ActivityPublishHandler), ServiceLifetime.Transient) { CanAddMultiple = true };
    }
}
