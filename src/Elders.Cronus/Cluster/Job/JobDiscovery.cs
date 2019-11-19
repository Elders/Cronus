using Elders.Cronus.Discoveries;
using Elders.Cronus.EventStore.Index;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Cluster.Job.InMemory
{
    public class JobDiscovery : DiscoveryBase<ICronusJob<object>>
    {
        protected override DiscoveryResult<ICronusJob<object>> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<ICronusJob<object>>(GetModels(context), AddServices);
        }

        private IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
        {
            var allTypes = context.Assemblies.SelectMany(ass => ass.GetExportedTypes());
            var cronusJobs = allTypes.Where(x => typeof(ICronusJob<object>).IsAssignableFrom(x) && x.IsAbstract == false && x.IsInterface == false);

            foreach (var job in cronusJobs)
            {
                yield return new DiscoveredModel(job, job, ServiceLifetime.Transient);
            }

            yield return new DiscoveredModel(typeof(TypeContainer<ICronusJob<object>>), new TypeContainer<ICronusJob<object>>(cronusJobs));
        }

        private void AddServices(IServiceCollection services)
        {
            services.AddTransient<ICronusJobRunner, InMemoryCronusJobRunner>();
            services.AddTransient<RebuildIndex_EventToAggregateRootId_JobFactory>();
        }
    }
}
