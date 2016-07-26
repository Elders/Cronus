using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Logging;
using Elders.Cronus.Middleware;

namespace Elders.Cronus.MessageProcessingMiddleware
{
    public class DynamicMessageHandle : Middleware<MessageHandlerMiddleware.HandlerContext>
    {
        protected override void Run(Execution<MessageHandlerMiddleware.HandlerContext> execution)
        {
            dynamic handler = execution.Context.HandlerInstance;
            handler.Handle((dynamic)execution.Context.Message);
        }
    }

    public class MessageHandlerMiddleware : Middleware<HandleContext>
    {
        static ILog log = LogProvider.GetLogger(typeof(MessageHandlerMiddleware));
        public Middleware<Type, IHandlerInstance> CreateHandler { get; private set; }

        public Middleware<HandlerContext> BeginHandle { get; private set; }

        public Middleware<HandlerContext> EndHandle { get; private set; }

        /// <summary>
        /// Why cant we inject this from the constructor?
        /// </summary>
        public Middleware<HandlerContext> ActualHandle { get; private set; }

        public void OnHandle(Func<Middleware<HandlerContext>, Middleware<HandlerContext>> handle)
        {
            ActualHandle = handle(ActualHandle);
        }

        public Middleware<ErrorContext> Error { get; private set; }

        public MessageHandlerMiddleware(IHandlerFactory factory)
        {
            CreateHandler = MiddlewareExtensions.Lambda<Type, IHandlerInstance>((execution) => factory.Create(execution.Context));

            BeginHandle = MiddlewareExtensions.Lamda<HandlerContext>();

            ActualHandle = new DynamicMessageHandle();

            EndHandle = MiddlewareExtensions.Lamda<HandlerContext>();

            Error = MiddlewareExtensions.Lamda<ErrorContext>();
        }
        protected override void Run(Execution<HandleContext> execution)
        {
            try
            {
                using (var handler = CreateHandler.Run(execution.Context.HandlerType))
                {
                    var handleContext = new HandlerContext(execution.Context.Message.Payload, handler.Current);

                    BeginHandle.Run(handleContext);
                    ActualHandle.Run(handleContext);
                    EndHandle.Run(handleContext);
                }
            }
            catch (Exception ex)
            {
                Error.Run(new ErrorContext(ex, execution.Context.Message, execution.Context.HandlerType));
            }
        }

        public class ErrorContext
        {
            public ErrorContext(Exception error, CronusMessage message, Type handlerType)
            {
                Error = error;
                Message = message;
                HandlerType = handlerType;
            }

            public Exception Error { get; private set; }

            public CronusMessage Message { get; private set; }

            public Type HandlerType { get; private set; }
        }

        public class HandlerContext
        {
            public HandlerContext(IMessage message, object handlerInstance)
            {
                Message = message;
                HandlerInstance = handlerInstance;
            }
            public IMessage Message { get; private set; }

            public object HandlerInstance { get; private set; }
        }
    }
}
