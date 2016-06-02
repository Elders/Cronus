using System;
using Elders.Cronus.MessageProcessingMiddleware;
using Elders.Cronus.Middleware;
using Netflix.Hystrix;
using static Elders.Cronus.MessageProcessingMiddleware.MessageHandlerMiddleware;

namespace Elders.Cronus.Hystrix
{
    public class HystrixMiddleware : Middleware.Middleware<HandleContext>
    {
        Middleware.Middleware<HandleContext> actualHandle;
        public HystrixMiddleware(Middleware.Middleware<HandleContext> actualHandle)
        {
            this.actualHandle = actualHandle;
        }

        protected override void Run(Execution<HandleContext> context)
        {
            var cmd = new HandleMessageHystrixCommand(actualHandle, context);
            cmd.Execute();
        }
    }

    public class HandleMessageHystrixCommand : HystrixCommand<bool>
    {
        Middleware.Middleware<HandleContext> actualHandle;
        Execution<HandleContext> context;

        public HandleMessageHystrixCommand(Middleware.Middleware<HandleContext> actualHandle, Execution<HandleContext> context)
            : base(HystrixCommandSetter.WithGroupKey("TimeGroup")
                .AndCommandKey("GetCurrentTime")
                .AndCommandPropertiesDefaults(
                    new HystrixCommandPropertiesSetter()
                    .WithExecutionIsolationThreadTimeout(TimeSpan.FromSeconds(1.0))
                    .WithExecutionIsolationStrategy(ExecutionIsolationStrategy.Semaphore)
                    .WithExecutionIsolationThreadInterruptOnTimeout(true)))
        {
            this.actualHandle = actualHandle;
            this.context = context;

        }

        protected override bool Run()
        {
            actualHandle.Run(context.Context);
            return true;
        }


    }
}
