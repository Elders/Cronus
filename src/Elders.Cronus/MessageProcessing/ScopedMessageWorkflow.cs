﻿using System;
using System.Collections.Concurrent;
using Elders.Cronus.Multitenancy;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.MessageProcessing
{
    public class ScopedMessageWorkflow : Workflow<HandleContext>
    {
        private static ConcurrentDictionary<HandleContext, IServiceScope> scopes = new ConcurrentDictionary<HandleContext, IServiceScope>();
        public static IServiceScope GetScope(HandleContext context) => scopes[context];

        private readonly IServiceProvider ioc;
        readonly Workflow<HandleContext> workflow;
        private readonly ITenantResolver tenantResolver;

        public ScopedMessageWorkflow(IServiceProvider ioc, Workflow<HandleContext> workflow, ITenantResolver tenantResolver)
        {
            this.ioc = ioc;
            this.workflow = workflow;
            this.tenantResolver = tenantResolver;
        }

        protected override void Run(Execution<HandleContext> execution)
        {
            using (IServiceScope scope = ioc.CreateScope())
            {
                EnsureTenantIsSet(scope, execution.Context.Message);

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

        void EnsureTenantIsSet(IServiceScope scope, CronusMessage message)
        {
            var cronusContext = scope.ServiceProvider.GetRequiredService<CronusContext>();
            if (cronusContext.IsNotInitialized)
            {
                string tenant = tenantResolver.Resolve(message);
                cronusContext.Initialize(tenant, scope.ServiceProvider);
            }
        }
    }
}
