//using Elders.Cronus.Middleware;

//namespace Elders.Cronus.MessageProcessing
//{
//    public class PortsMiddleware : MessageHandlerMiddleware
//    {
//        public PortsMiddleware(IHandlerFactory factory, IPublisher<ICommand> commandPublisher)
//            : base(factory)
//        {
//            BeginHandle.Use((execution) =>
//            {
//                IPublisher<ICommand> cronusCommandPublisher = new CronusPublisher<ICommand>(commandPublisher, execution.Context.CronusMessage);
//                execution.Context.HandlerInstance.AssignPropertySafely<IPort>(x => x.CommandPublisher = cronusCommandPublisher);
//            });
//        }
//    }
//}
