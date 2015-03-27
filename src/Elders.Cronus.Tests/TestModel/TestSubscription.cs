using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Tests.TestModel
{
    public class TestSubscription : MessageProcessorSubscription
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(TestSubscription));

        public TestSubscription(Type messageType, IHandlerFactory factory)
            : base("Elders.Cronus.Tests", messageType, factory.MessageHandlerType)
        {
            Factory = factory;
        }

        public IHandlerFactory Factory { get; private set; }

        protected override void InternalOnNext(Message value)
        {
            object handler = Factory.Create();
            ((dynamic)handler).Handle((dynamic)value.Payload);

            log.Info("HANDLE => " + Factory.MessageHandlerType.Name + "( " + value.Payload + " )");
        }
    }
}
