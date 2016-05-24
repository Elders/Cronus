using System;
using Elders.Cronus.MessageProcessingMiddleware;

namespace Elders.Cronus.Tests.TestModel
{
    public class TestSubscriber : SubscriberMiddleware
    {
        public TestSubscriber(Type messageType, Type handlerType, MessageHandlerMiddleware handlerMiddleware)
            : base("Elders.Cronus.Tests", messageType, handlerType, handlerMiddleware)
        {
        }
    }
}
