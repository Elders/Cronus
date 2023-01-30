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
        //yield return new DiscoveredModel(typeof(ILock), typeof(InMemoryLockWithTTL), ServiceLifetime.Transient);
        // yield return new DiscoveredModel(typeof(IPublisher<>), typeof(InMemoryPublisher<>), ServiceLifetime.Singleton);
        yield return new DiscoveredModel(typeof(IAggregateRootAtomicAction), typeof(InMemoryAggregateRootAtomicAction), ServiceLifetime.Transient);

        //yield return new DiscoveredModel(typeof(IConsumer<>), typeof(EmptyConsumer<>), ServiceLifetime.Singleton);
    }
}
