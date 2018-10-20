using System;
using Elders.Cronus.EventStore;
using Elders.Cronus.Logging;
using Elders.Cronus.Middleware;
using Elders.Cronus.Multitenancy;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.MessageProcessing
{
    public sealed class MessageHandlerMiddleware : Middleware<HandleContext>
    {
        static ILog log = LogProvider.GetLogger(typeof(MessageHandlerMiddleware));

        private readonly IServiceProvider ioc;

        public Middleware<Type, IHandlerInstance> CreateHandler { get; private set; }

        public Middleware<HandlerContext> BeginHandle { get; private set; }

        public Middleware<HandlerContext> EndHandle { get; private set; }

        public Middleware<ErrorContext> Error { get; private set; }

        public Middleware<HandleContext> Finalize { get; private set; }

        public Middleware<HandlerContext> ActualHandle { get; private set; }

        public void OnHandle(Func<Middleware<HandlerContext>, Middleware<HandlerContext>> handle)
        {
            ActualHandle = handle(ActualHandle);
        }

        public MessageHandlerMiddleware(IServiceProvider ioc)
        {
            CreateHandler = MiddlewareExtensions.Lambda<Type, IHandlerInstance>();

            BeginHandle = MiddlewareExtensions.Lamda<HandlerContext>();

            ActualHandle = new DynamicMessageHandle();

            EndHandle = MiddlewareExtensions.Lamda<HandlerContext>();

            Error = MiddlewareExtensions.Lamda<ErrorContext>();

            Finalize = MiddlewareExtensions.Lamda<HandleContext>();

            this.ioc = ioc;
        }

        protected override void Run(Execution<HandleContext> execution)
        {
            using (IServiceScope scope = ioc.CreateScope())
            {
                try
                {
                    string tenant = execution.Context.Message.GetTenant();
                    if (string.IsNullOrEmpty(tenant)) throw new Exception($"Unable to resolve tenant from message {execution.Context.Message}");

                    var cronusContext = scope.ServiceProvider.GetRequiredService<CronusContext>();
                    cronusContext.Tenant = tenant;

                    IHandlerFactory factory = new DefaultHandlerFactory(type => scope.ServiceProvider.GetRequiredService(type));
                    CreateHandler = MiddlewareExtensions.Lambda<Type, IHandlerInstance>((exec) => factory.Create(exec.Context));

                    using (IHandlerInstance handler = CreateHandler.Run(execution.Context.HandlerType))
                    {
                        var handleContext = new HandlerContext(execution.Context.Message.Payload, handler.Current, execution.Context.Message);

                        BeginHandle.Run(handleContext);
                        ActualHandle.Run(handleContext);
                        EndHandle.Run(handleContext);
                    }
                }

                catch (Exception ex)
                {
                    Error.Run(new ErrorContext(ex, execution.Context.Message, execution.Context.HandlerType));
                    throw;
                }
                finally
                {
                    Finalize.Run(execution.Context);
                }
            }
        }
    }
}
