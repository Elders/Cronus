//using System.Collections.Generic;
//using Elders.Cronus.AtomicAction;
//using Elders.Cronus.AtomicAction.InMemory;
//using Elders.Cronus.InMemory;
//using Microsoft.Extensions.DependencyInjection;

//namespace Elders.Cronus.Discoveries
//{
//    public class InMemoryDiscovery : DiscoveryBasedOnExecutingDirAssemblies<IPublisher<IMessage>>
//    {
//        protected override DiscoveryResult<IPublisher<IMessage>> DiscoverFromAssemblies(DiscoveryContext context)
//        {
//            return new DiscoveryResult<IPublisher<IMessage>>(GetModels());
//        }

//        IEnumerable<DiscoveredModel> GetModels()
//        {
            //yield return new DiscoveredModel(typeof(InMemoryQueue), typeof(InMemoryQueue), ServiceLifetime.Singleton);
            yield return new DiscoveredModel(typeof(IPublisher<>), typeof(SynchronousMessageProcessor<>), ServiceLifetime.Singleton);
            //yield return new DiscoveredModel(typeof(IConsumer<>), typeof(InMemoryConsumer<>), ServiceLifetime.Transient);
//            yield return new DiscoveredModel(typeof(IAggregateRootAtomicAction), typeof(InMemoryAggregateRootAtomicAction), ServiceLifetime.Transient);
//        }
//    }
//}
