using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Tests.TestModel;
using Machine.Specifications;

namespace Elders.Cronus.Tests.MessageStreaming
{
    [Subject("")]
    public class When_message_handler_implements__IDisposable__
    {
        Establish context = () =>
            {
                handlerFacotry = new DisposableHandlerFactory();
                messageStream = new MessageProcessor("test");
                var subscription = new TestSubscription(typeof(CalculatorNumber1), handlerFacotry);

                messages = new List<TransportMessage>();
                messages.Add(new TransportMessage(new Message(new CalculatorNumber1(1))));
                messageStream.Subscribe(subscription);
            };

        Because of = () =>
            {
                feedResult = messageStream.Feed(messages);
            };

        It should_dispose_handler_resources_if_possible = () => (handlerFacotry.State.Current as DisposableHandlerFactory.DisposableHandler).IsDisposed.ShouldBeTrue();

        static IFeedResult feedResult;
        static MessageProcessor messageStream;
        static List<TransportMessage> messages;
        static DisposableHandlerFactory handlerFacotry;
    }

    public class DisposableHandlerFactory : IHandlerFactory
    {
        public Type MessageHandlerType { get { return typeof(DisposableHandler); } }

        public IHandlerInstance State { get; set; }

        public IHandlerInstance Create()
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
