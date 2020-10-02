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
            IEnumerable<DiscoveredModel> models =
                DiscoverCronusHost(context)
                .Concat(DiscoverCronusStartups(context))
                .Concat(DiscoverCommands(context))
                .Concat(DiscoverEvents(context))
                .Concat(DiscoverPublicEvents(context))
                .Concat(DiscoverSignals(context));

            return new DiscoveryResult<ICronusHost>(models);
        }

        protected virtual IEnumerable<DiscoveredModel> DiscoverCronusHost(DiscoveryContext context)
        {
            return DiscoverModel<ICronusHost, CronusHost>(ServiceLifetime.Transient);
        }

        protected virtual IEnumerable<DiscoveredModel> DiscoverCronusStartups(DiscoveryContext context)
        {
            foreach (var startupType in context.Assemblies.Find<ICronusStartup>())
            {
                yield return new DiscoveredModel(startupType, startupType, ServiceLifetime.Singleton);
            }

            yield return new DiscoveredModel(typeof(Cronus.ProjectionsStartup), typeof(Cronus.ProjectionsStartup), ServiceLifetime.Transient); // TODO: Check if this is alrady registered in the foreach above. If yes we can remove this line. Elase we have to figure out what is going on
        }

        protected virtual IEnumerable<DiscoveredModel> DiscoverCommands(DiscoveryContext context)
        {
            IEnumerable<Type> loadedCommands = context.Assemblies.Find<ICommand>();
            yield return new DiscoveredModel(typeof(TypeContainer<ICommand>), new TypeContainer<ICommand>(loadedCommands));
        }

        protected virtual IEnumerable<DiscoveredModel> DiscoverEvents(DiscoveryContext context)
        {
            IEnumerable<Type> loadedCommands = context.Assemblies.Find<ICommand>();
            yield return new DiscoveredModel(typeof(TypeContainer<ICommand>), new TypeContainer<ICommand>(loadedCommands));
        }

        protected virtual IEnumerable<DiscoveredModel> DiscoverPublicEvents(DiscoveryContext context)
        {
            IEnumerable<Type> loadedPublicEvents = context.Assemblies.Find<IPublicEvent>();
            yield return new DiscoveredModel(typeof(TypeContainer<IPublicEvent>), new TypeContainer<IPublicEvent>(loadedPublicEvents));
        }

        protected virtual IEnumerable<DiscoveredModel> DiscoverSignals(DiscoveryContext context)
        {
            IEnumerable<Type> loadedSignals = context.Assemblies.Find<ISignal>();
            yield return new DiscoveredModel(typeof(TypeContainer<ISignal>), new TypeContainer<ISignal>(loadedSignals));
        }
    }
}
