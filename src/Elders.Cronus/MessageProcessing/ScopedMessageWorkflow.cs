using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static System.Formats.Asn1.AsnWriter;

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

        protected override Execution<HandleContext> CreateExecutionContext(HandleContext context)
        {
            bool hasScopeError = false;
            if (context.ServiceProvider is null)
            {
                IServiceScope scope = default;
                if (scopes.TryGetValue(context, out scope) == false)
                {
                    scope = ioc.CreateScope();
                    if (scopes.TryAdd(context, scope) == false)
                    {
                        hasScopeError = true;
                    }
                }

                context.ServiceProvider = scope.ServiceProvider;
            }

            ILogger<ScopedMessageWorkflow> logger = context.ServiceProvider.GetRequiredService<ILogger<ScopedMessageWorkflow>>();
            context.LoggerScope = logger.BeginScope(scope => scope.AddScope("cronus_tenant", context.Message.GetTenant()));
            if (hasScopeError)
                logger.Critical(() => "Somehow the IServiceScope has been already created and there will be an unexpected behavior after this message.");

            return base.CreateExecutionContext(context);
        }

        protected override Task OnRunCompletedAsync(Execution<HandleContext> execution)
        {
            if (scopes.TryRemove(execution.Context, out IServiceScope s))
            {
                execution.Context.ServiceProvider = null;
                s.Dispose();

                if (execution.Context.ServiceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            execution.Context.LoggerScope?.Dispose();

            return Task.CompletedTask;
        }

        protected override async Task RunAsync(Execution<HandleContext> execution)
        {
            if (EnsureTenantIsSet(execution.Context.ServiceProvider, execution.Context.Message))
            {
                await workflow.RunAsync(execution.Context).ConfigureAwait(false);
            }
        }

        private bool EnsureTenantIsSet(IServiceProvider serviceProvider, CronusMessage message)
        {
            var cronusContextFactory = serviceProvider.GetRequiredService<CronusContextFactory>();
            var context = cronusContextFactory.GetContext(message, serviceProvider);

            foreach (var header in message.Headers)
            {
                context.Trace.Add(header.Key, header.Value);
            }

            return context.IsInitialized;
        }
    }
}
