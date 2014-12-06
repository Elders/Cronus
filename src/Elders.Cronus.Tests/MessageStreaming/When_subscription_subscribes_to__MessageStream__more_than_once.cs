using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.IocContainer;
using Machine.Specifications;

namespace Elders.Cronus.Tests.MessageStreaming
{
    [Subject("")]
    public class When_subscription_subscribes_to__MessageStream__more_than_once
    {
        Establish context = () =>
            {
                HandleResult.Total = 0;
                IContainer container = new Container();
                messageStream = new MessageStream(container);
                var subscription1 = new Subscription(typeof(TestIntMessage1), typeof(TestAddHandler1), (t) => FastActivator.CreateInstance(t));

                messages = new List<TransportMessage>();
                for (int i = 0; i < numberOfMessages; i++)
                {
                    messages.Add(new TransportMessage(new TestIntMessage1(i)));
                }
                messageStream.Subscribe(subscription1);
                messageStream.Subscribe(subscription1);
                messageStream.Subscribe(subscription1);
                messageStream.Subscribe(subscription1);
            };

        Because of = () =>
            {
                messageStream.Feed(messages);
            };

        It should_accept_only_the_first_subscription = () => HandleResult.Total.ShouldEqual(Enumerable.Range(0, numberOfMessages).Sum());

        static int numberOfMessages = 100;
        static MessageStream messageStream;
        static List<TransportMessage> messages;
    }
}