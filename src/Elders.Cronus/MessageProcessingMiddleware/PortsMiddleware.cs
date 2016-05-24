using Elders.Cronus.DomainModeling;
using Elders.Cronus.Middleware;

namespace Elders.Cronus.MessageProcessingMiddleware
{
    public class PortsMiddleware : MessageHandlerMiddleware
    {
        private readonly IPublisher<ICommand> commandPublisher;

        public PortsMiddleware(IHandlerFactory factory, IPublisher<ICommand> commandPublisher)
            : base(factory)
        {
            this.commandPublisher = commandPublisher;
            BeginHandle.Next((context, execution) =>
            {
                context.HandlerInstance.AssignPropertySafely<IPort>(x => x.CommandPublisher = this.commandPublisher);
            });
        }
    }
}