using System;
using Elders.Cronus.Workflow;

namespace Elders.Cronus.MessageProcessing
{
    /// <summary>
    /// A work-flow which gives you the ability to call Handle on an instance object for a message. A 'Workflow<HandleContext, IHandlerInstance>' should be passed which
    /// would be used for instantiating a new instance of the desired object which would handle the message.
    /// </summary>
    public sealed class MessageHandleWorkflow : Workflow<HandleContext>
    {
        public MessageHandleWorkflow() : this(DefaultHandlerFactory.FactoryWrokflow) { }

        public MessageHandleWorkflow(Workflow<HandleContext, IHandlerInstance> createHandler)
        {
            CreateHandler = createHandler;
            BeginHandle = WorkflowExtensions.Lamda<HandlerContext>();
            ActualHandle = WorkflowExtensions.Lamda<HandlerContext>().Use(context => new DynamicMessageHandle().Run(context.Context));
            EndHandle = WorkflowExtensions.Lamda<HandlerContext>();
            Error = WorkflowExtensions.Lamda<ErrorContext>().Use(context => new LogExceptionOnHandleError().Run(context.Context));
            Finalize = WorkflowExtensions.Lamda<HandleContext>();
        }

        public Workflow<HandleContext, IHandlerInstance> CreateHandler { get; private set; }

        /// <summary>
        /// Work-flow which would be executed at the beginning of the work-flow.
        /// By default there is no work-flow set. If you want you can call 'Override' to attach different message handler.
        /// </summary>
        public Workflow<HandlerContext> BeginHandle { get; private set; }

        /// <summary>
        /// Work-flow which would be executed after the 'ActualHandle' and 'BeginHandle' are executed.
        /// By default there is no work-flow set. If you want you can call 'Override' to attach different message handler.
        /// </summary>
        public Workflow<HandlerContext> EndHandle { get; private set; }

        /// <summary>
        /// Work-flow which would be executed on an exception of raised by 'BeginHandle', 'ActualHandle' or 'EndHandle' work-flows.
        /// By default there is no work-flow set. If you want you can call 'Override' to attach different message handler.
        /// </summary>
        public Workflow<ErrorContext> Error { get; private set; }

        /// <summary>
        /// Work-flow which would be executed after the run has finished even if there is an error on it.
        /// By default there is no work-flow set.
        /// </summary>
        public Workflow<HandleContext> Finalize { get; private set; }

        /// <summary>
        /// Work-flow which would actually call handle on the target instance.
        /// The default work-flow used is 'DynamicMessageHandle'. If you want you can call 'Override' to attach different actual message handler
        /// </summary>
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
                var context = new ErrorContext(ex, execution.Context.Message, execution.Context.HandlerType);
                context.AssignPropertySafely<IWorkflowContextWithServiceProvider>(prop => prop.ServiceProvider = execution.Context.ServiceProvider);
                Error.Run(context);
                throw;
            }
            finally
            {
                Finalize.Run(execution.Context);
            }
        }
    }
}
