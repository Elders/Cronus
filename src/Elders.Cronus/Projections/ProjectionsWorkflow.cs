using System;
using Elders.Cronus.Workflow;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Projections
{
    //public interface IProjectionWriterProvider
    //{
    //    public IProjectionWriter GetInstance(HandleContext ctx);
    //}
    public class ProjectionsWorkflow : Workflow<HandleContext>
    {
        readonly Func<HandleContext, IProjectionWriter> projectionWriterProvider;

        public ProjectionsWorkflow(Func<HandleContext, IProjectionWriter> projectionWriterProvider)
        {
            if (projectionWriterProvider is null) throw new ArgumentNullException(nameof(projectionWriterProvider));

            this.projectionWriterProvider = projectionWriterProvider;
        }

        protected override void Run(Execution<HandleContext> execution)
        {
            var projectionWriter = projectionWriterProvider(execution.Context);
            CronusMessage cronusMessage = execution.Context.Message;
            if (cronusMessage.Payload is IEvent)
            {
                Type projectionType = execution.Context.HandlerType;
                projectionWriter.Save(projectionType, cronusMessage);
            }
        }
    }
}
