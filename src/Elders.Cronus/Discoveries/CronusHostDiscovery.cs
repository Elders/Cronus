using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public class CronusHostDiscovery : DiscoveryBase<ICronusHost>
    {
        protected override DiscoveryResult<ICronusHost> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<ICronusHost>(GetModels(context));
        }

        IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
        {
            foreach (var startupType in context.Assemblies.Find<ICronusStartup>())
            {
                yield return new DiscoveredModel(startupType, startupType, ServiceLifetime.Singleton);
            }

            yield return new DiscoveredModel(typeof(ICronusHost), typeof(CronusHost), ServiceLifetime.Transient);

            yield return new DiscoveredModel(typeof(Cronus.ProjectionsStartup), typeof(Cronus.ProjectionsStartup), ServiceLifetime.Transient);

            IEnumerable<Type> loadedCommands = context.Assemblies.Find<ICommand>();
            yield return new DiscoveredModel(typeof(TypeContainer<ICommand>), new TypeContainer<ICommand>(loadedCommands));

            IEnumerable<Type> loadedEvents = context.Assemblies.Find<IEvent>().Where(type => type != typeof(EntityEvent));
            yield return new DiscoveredModel(typeof(TypeContainer<IEvent>), new TypeContainer<IEvent>(loadedEvents));

            IEnumerable<Type> loadedPublicEvents = context.Assemblies.Find<IPublicEvent>();
            yield return new DiscoveredModel(typeof(TypeContainer<IPublicEvent>), new TypeContainer<IPublicEvent>(loadedPublicEvents));

            IEnumerable<Type> loadedSignals = context.Assemblies.Find<ISignal>();
            yield return new DiscoveredModel(typeof(TypeContainer<ISignal>), new TypeContainer<ISignal>(loadedSignals));
        }
    }
}
