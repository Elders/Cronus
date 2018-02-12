using Elders.Cronus.Middleware;

namespace Elders.Cronus.MessageProcessing
{
    public class SagasMiddleware : MessageHandlerMiddleware
    {
        public SagasMiddleware(IHandlerFactory factory, IPublisher<ICommand> commandPublisher, IPublisher<IScheduledMessage> schedulePublisher)
            : base(factory)
        {
            BeginHandle.Use((execution) =>
            {
                IPublisher<ICommand> cronusCommandPublisher = new CronusPublisher<ICommand>(commandPublisher, execution.Context.CronusMessage);
                IPublisher<IScheduledMessage> cronusSchedulePublisher = new CronusPublisher<IScheduledMessage>(schedulePublisher, execution.Context.CronusMessage);

                execution.Context.HandlerInstance.AssignPropertySafely<ISaga>(x => x.CommandPublisher = cronusCommandPublisher);
                execution.Context.HandlerInstance.AssignPropertySafely<ISaga>(x => x.TimeoutRequestPublisher = cronusSchedulePublisher);
            });
        }
    }
}
