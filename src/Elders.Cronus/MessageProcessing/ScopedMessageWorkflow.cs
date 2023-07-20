using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.MessageProcessing
{
    public class ScopedMessageWorkflow : Workflow<HandleContext>
    {
        private static ConcurrentDictionary<HandleContext, IServiceScope> scopes = new ConcurrentDictionary<HandleContext, IServiceScope>();
        public static IServiceScope GetScope(HandleContext context) => scopes[context];

        private readonly DefaultCronusContextFactory cronusContextFactory;
        readonly Workflow<HandleContext> workflow;
        private readonly IServiceScopeFactory serviceScopeFactory;

        public ScopedMessageWorkflow(Workflow<HandleContext> workflow, IServiceProvider serviceProvider)
        {
            this.workflow = workflow;
            this.cronusContextFactory = serviceProvider.GetRequiredService<DefaultCronusContextFactory>();
            this.serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        protected override Execution<HandleContext> CreateExecutionContext(HandleContext context)
        {
            string tenant = context.Message.GetTenant();

            ILogger<ScopedMessageWorkflow> logger = context.ServiceProvider.GetRequiredService<ILogger<ScopedMessageWorkflow>>();
            context.LoggerScope = logger.BeginScope(scope => scope.AddScope(Log.Tenant, tenant));

            bool hasScopeError = false;

            IServiceScope scope = default;
            if (scopes.TryGetValue(context, out scope) == false)
            {
                scope = serviceScopeFactory.CreateScope();
                if (scopes.TryAdd(context, scope) == false)
                    hasScopeError = true;

                var cronusContext = cronusContextFactory.Create(tenant, scope.ServiceProvider);
                foreach (var header in context.Message.Headers)
                {
                    cronusContext.Trace.Add(header.Key, header.Value);
                }
            }

            context.ServiceProvider = scope.ServiceProvider;

            if (hasScopeError)
            {
                string message = "Somehow the IServiceScope has been already created and there will be an unexpected behavior after this message.";
                logger.Critical(() => message);
                throw new Exception(message);
            }

            return base.CreateExecutionContext(context);
        }

        protected override Task OnRunCompletedAsync(Execution<HandleContext> execution)
        {
            if (scopes.TryRemove(execution.Context, out IServiceScope serviceScope))
            {
                execution.Context.ServiceProvider = null;
                serviceScope.Dispose();

                if (execution.Context.ServiceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            execution.Context.LoggerScope?.Dispose();

            return Task.CompletedTask;
        }

        protected override Task RunAsync(Execution<HandleContext> execution)
        {
            return workflow.RunAsync(execution.Context);
        }
    }
}
