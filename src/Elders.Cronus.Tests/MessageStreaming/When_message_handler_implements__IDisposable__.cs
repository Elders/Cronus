using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Tests.TestModel;
using Machine.Specifications;
using Elders.Cronus.MessageProcessingMiddleware;

namespace Elders.Cronus.Tests.MessageStreaming
{
    [Subject("")]
    public class When_message_handler_implements__IDisposable__
    {
        Establish context = () =>
            {
                handlerFacotry = new DisposableHandlerFactory();
                var messageHandlerMiddleware = new MessageHandlerMiddleware(handlerFacotry);
                var messageSubscriptionMiddleware = new MessageSubscriptionsMiddleware();
                messageStream = new CronusMessageProcessorMiddleware("test", messageSubscriptionMiddleware);


                var subscription = new TestSubscriber(typeof(CalculatorNumber1), typeof(DisposableHandlerFactory.DisposableHandler), messageHandlerMiddleware);



                messages = new List<CronusMessage>();
                messages.Add(new CronusMessage(new Message(new CalculatorNumber1(1))));
                messageSubscriptionMiddleware.Subscribe(subscription);
            };

        Because of = () =>
            {
                feedResult = messageStream.Run(messages);
            };

        It should_dispose_handler_resources_if_possible = () => (handlerFacotry.State.Current as DisposableHandlerFactory.DisposableHandler).IsDisposed.ShouldBeTrue();

        static IFeedResult feedResult;
        static CronusMessageProcessorMiddleware messageStream;
        static List<CronusMessage> messages;
        static DisposableHandlerFactory handlerFacotry;
    }

    public class DisposableHandlerFactory : IHandlerFactory
    {
        public IHandlerInstance State { get; set; }

        public IHandlerInstance Create(Type handlerType)
        {
            State = new DefaultHandlerInstance(new DisposableHandler());
            return State;
        }

        public class DisposableHandler : IDisposable
        {
            public bool IsDisposed { get; set; }

            public void Handle(CalculatorNumber1 @event) { }

            public void Dispose() { IsDisposed = true; }
        }
    }
}
