using System;
using System.Collections.Concurrent;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.MessageProcessing
{
    public class ScopedMessageWorkflow : Workflow<HandleContext>
    {
        private static ConcurrentDictionary<HandleContext, IServiceScope> scopes = new ConcurrentDictionary<HandleContext, IServiceScope>();
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
                ILogger<ScopedMessageWorkflow> logger = scope.ServiceProvider.GetRequiredService<ILogger<ScopedMessageWorkflow>>();
                if (EnsureTenantIsSet(scope, execution.Context.Message))
                {
                    using (logger.BeginScope(scope => scope
                        .AddScope("cronus_tenant", execution.Context.Message.GetTenant())))
                    {
                        scopes.AddOrUpdate(execution.Context, scope, (c, s) => scope);
                        try
                        {
                            workflow.Run(execution.Context);
                        }
                        finally
                        {
                            scopes.TryRemove(execution.Context, out IServiceScope s);
                        }
                    }
                }
            }
        }

        bool EnsureTenantIsSet(IServiceScope scope, CronusMessage message)
        {
            var cronusContextFactory = scope.ServiceProvider.GetRequiredService<CronusContextFactory>();
            var context = cronusContextFactory.GetContext(message, scope.ServiceProvider);

            foreach (var header in message.Headers)
            {
                context.Trace.Add(header.Key, header.Value);
            }

            return context.IsInitialized;
        }
    }
}
