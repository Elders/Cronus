using System;
using System.Collections.Generic;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public interface ICronusApiAccessor
    {
        IServiceProvider Provider { get; set; }
    }

    public class CronusApiAccessor : ICronusApiAccessor
    {
        public IServiceProvider Provider { get; set; }
    }

    public class CronusWorkflowsDiscovery : DiscoveryBase<IWorkflow>
    {
        protected override DiscoveryResult<IWorkflow> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<IWorkflow>(DiscoverWorkflows(context), RegisterGG);
        }

        protected virtual IEnumerable<DiscoveredModel> DiscoverWorkflows(DiscoveryContext context)
        {
            return DiscoverModel<Workflow<HandleContext>, MessageHandleWorkflow>(ServiceLifetime.Transient);
        }

        public void RegisterGG(IServiceCollection services)
        {
            services.AddSingleton<ICronusApiAccessor, CronusApiAccessor>();
        }
    }
}
