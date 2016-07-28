using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.MessageProcessing
{
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
