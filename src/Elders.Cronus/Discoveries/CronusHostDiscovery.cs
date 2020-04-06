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

            yield return new DiscoveredModel(typeof(BoundedContext), typeof(BoundedContext), ServiceLifetime.Transient);

            var loadedCommands = context.Assemblies.Find<ICommand>();
            yield return new DiscoveredModel(typeof(TypeContainer<ICommand>), new TypeContainer<ICommand>(loadedCommands));

            var loadedEvents = context.Assemblies.Find<IEvent>().Where(type => type != typeof(EntityEvent));
            yield return new DiscoveredModel(typeof(TypeContainer<IEvent>), new TypeContainer<IEvent>(loadedEvents));
        }
    }
}
