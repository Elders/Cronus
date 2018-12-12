using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            yield return new DiscoveredModel(typeof(Cronus.ProjectionsBooter), typeof(Cronus.ProjectionsBooter), ServiceLifetime.Transient);

            yield return new DiscoveredModel(typeof(BoundedContext), typeof(BoundedContext), ServiceLifetime.Transient);

            var loadedTypes = context.Assemblies.Find<IEvent>().Where(type => type != typeof(EntityEvent));
            yield return new DiscoveredModel(typeof(TypeContainer<IEvent>), new TypeContainer<IEvent>(loadedTypes));
        }
    }

    public static class DiscoveryAssemblyExtensions
    {
        public static IEnumerable<Type> Find<TService>(this IEnumerable<Assembly> assemblies)
        {
            return assemblies
                .SelectMany(asm => asm.GetLoadableTypes())
                .Where(type => type.IsAbstract == false)
                .Where(type => type.IsInterface == false)
                .Where(type => typeof(TService).IsAssignableFrom(type));
        }
    }
}
