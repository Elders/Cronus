using System;
using Elders.Cronus.Middleware;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Projections
{
    public class EventSourcedProjectionsMiddleware : Middleware<HandleContext>
    {
        readonly IProjectionWriter repository;

        public EventSourcedProjectionsMiddleware(IProjectionWriter repository)
        {
            if (ReferenceEquals(null, repository) == true) throw new ArgumentNullException(nameof(repository));

            this.repository = repository;
        }

        protected override void Run(Execution<HandleContext> execution)
        {
            CronusMessage cronusMessage = execution.Context.Message;
            if (cronusMessage.Payload is IEvent)
            {
                Type projectionType = execution.Context.HandlerType;
                repository.Save(projectionType, cronusMessage);
            }
        }
    }
}
