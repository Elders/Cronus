using System;

namespace Elders.Cronus.MessageProcessing
{
    public class ErrorContext
    {
        public ErrorContext(Exception error, CronusMessage message, Type handlerType)
        {
            Error = error;
            Message = message;
            HandlerType = handlerType;
        }

        public Exception Error { get; private set; }

        public CronusMessage Message { get; private set; }

        public Type HandlerType { get; private set; }
    }
}
