using System;
using Elders.Cronus.Workflow;

namespace Elders.Cronus.MessageProcessing
{
    public sealed class MessageHandleWorkflow : Workflow<HandleContext>
    {
        public MessageHandleWorkflow() : this(DefaultHandlerFactory.FactoryWrokflow) { }

        public MessageHandleWorkflow(Workflow<HandleContext, IHandlerInstance> createHandler)
        {
            CreateHandler = createHandler;
            BeginHandle = WorkflowExtensions.Lamda<HandlerContext>();
            ActualHandle = new DynamicMessageHandle();
            EndHandle = WorkflowExtensions.Lamda<HandlerContext>();
            Error = WorkflowExtensions.Lamda<ErrorContext>();
            Finalize = WorkflowExtensions.Lamda<HandleContext>();
        }

        public Workflow<HandleContext, IHandlerInstance> CreateHandler { get; private set; }

        public Workflow<HandlerContext> BeginHandle { get; private set; }

        public Workflow<HandlerContext> EndHandle { get; private set; }

        public Workflow<ErrorContext> Error { get; private set; }

        public Workflow<HandleContext> Finalize { get; private set; }

        public Workflow<HandlerContext> ActualHandle { get; private set; }

        public void OnHandle(Func<Workflow<HandlerContext>, Workflow<HandlerContext>> handle)
        {
            ActualHandle = handle(ActualHandle);
        }

        protected override void Run(Execution<HandleContext> execution)
        {
            try
            {
                using (IHandlerInstance handler = CreateHandler.Run(execution.Context))
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
