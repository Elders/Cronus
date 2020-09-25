using System.Collections.Generic;
using Elders.Cronus.Migrations;
using Elders.Cronus.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public class MigrationDiscovery : DiscoveryBase<MigrationDiscovery>
    {
        protected override DiscoveryResult<MigrationDiscovery> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<MigrationDiscovery>(GetModels(context));
        }

        IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
        {
            yield return new DiscoveredModel(typeof(CopyEventStore<,>), typeof(CopyEventStore<,>), ServiceLifetime.Transient);
        }
    }


    public class QueryProviderDiscovery : DiscoveryBase<QueryProvider>
    {
        protected override DiscoveryResult<QueryProvider> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<QueryProvider>(GetModels(context));
        }

        IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
        {
            IEnumerable<System.Type> foundTypes = context.FindService<IQueryHandlerDefinition>();
            foreach (var type in foundTypes)
            {
                yield return new DiscoveredModel(typeof(IQueryHandlerDefinition), type, ServiceLifetime.Transient); // singleton??

                yield return new DiscoveredModel(type, type, ServiceLifetime.Transient); // singleton??
            }
            

            yield return new DiscoveredModel(typeof(QueryProvider), typeof(QueryProvider), ServiceLifetime.Transient);
        }
    }
}
