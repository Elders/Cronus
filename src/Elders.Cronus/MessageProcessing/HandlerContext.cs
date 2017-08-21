namespace Elders.Cronus.MessageProcessing
{
    public class HandlerContext
    {
        public HandlerContext(IMessage message, object handlerInstance, CronusMessage cronusMessage)
        {
            Message = message;
            HandlerInstance = handlerInstance;
            CronusMessage = cronusMessage;
        }
        public IMessage Message { get; private set; }

        public object HandlerInstance { get; private set; }

        public CronusMessage CronusMessage { get; private set; }
    }
}
