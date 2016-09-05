using System;
using Elders.Cronus.Logging;
using Elders.Cronus.Middleware;

namespace Elders.Cronus.MessageProcessing
{
    public class MessageHandlerMiddleware : Middleware<HandleContext>
    {
        static ILog log = LogProvider.GetLogger(typeof(MessageHandlerMiddleware));

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

        public MessageHandlerMiddleware(IHandlerFactory factory)
        {
            CreateHandler = MiddlewareExtensions.Lambda<Type, IHandlerInstance>((execution) => factory.Create(execution.Context));

            BeginHandle = MiddlewareExtensions.Lamda<HandlerContext>();

            ActualHandle = new DynamicMessageHandle();

            EndHandle = MiddlewareExtensions.Lamda<HandlerContext>();

            Error = MiddlewareExtensions.Lamda<ErrorContext>();

            Finalize = MiddlewareExtensions.Lamda<HandleContext>();
        }

        protected override void Run(Execution<HandleContext> execution)
        {
            try
            {
                using (var handler = CreateHandler.Run(execution.Context.HandlerType))
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
