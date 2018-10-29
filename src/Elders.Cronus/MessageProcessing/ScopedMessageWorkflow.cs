using System;
using System.Collections.Generic;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.MessageProcessing
{
    public class ScopedMessageWorkflow : Workflow<HandleContext>
    {
        private static Dictionary<HandleContext, IServiceScope> scopes = new Dictionary<HandleContext, IServiceScope>();
        public static IServiceScope GetScope(HandleContext context) => scopes[context];

        private readonly IServiceProvider ioc;
        readonly Workflow<HandleContext> workflow;

        public ScopedMessageWorkflow(IServiceProvider ioc, Workflow<HandleContext> workflow)
        {
            this.ioc = ioc;
            this.workflow = workflow;
        }

        protected override void Run(Execution<HandleContext> execution)
        {
            using (IServiceScope scope = ioc.CreateScope())
            {
                EnsureTenantIsSet(scope, execution.Context.Message);

                scopes.Add(execution.Context, scope);
                try
                {
                    workflow.Run(execution.Context);
                }
                finally
                {
                    scopes.Remove(execution.Context);
                }
            }
        }

        void EnsureTenantIsSet(IServiceScope scope, CronusMessage message)
        {
            var cronusContext = scope.ServiceProvider.GetRequiredService<CronusContext>();
            if (cronusContext.IsNotInitialized)
            {
                string tenant = message.GetTenant();
                if (string.IsNullOrEmpty(tenant)) throw new Exception($"Unable to resolve tenant from {message}");
                cronusContext.Initialize(tenant, scope.ServiceProvider);
            }
        }
    }
}
