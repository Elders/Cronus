using System.Collections.Generic;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public class CronusWorkflowsDiscovery : DiscoveryBase<IWorkflow>
    {
        protected override DiscoveryResult<IWorkflow> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<IWorkflow>(DiscoverWorkflows(context));
        }

        protected virtual IEnumerable<DiscoveredModel> DiscoverWorkflows(DiscoveryContext context)
        {
            return DiscoverModel<Workflow<HandleContext>, MessageHandleWorkflow>(ServiceLifetime.Transient);
        }
    }
}
