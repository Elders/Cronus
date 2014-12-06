using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.IocContainer;
using Machine.Specifications;

namespace Elders.Cronus.Tests.MessageStreaming
{
    [Subject("")]
    public class When__MessageStream__receives_a_batch_of__TransportMessages__
    {
        Establish context = () =>
            {
                HandleResult.Total = 0;
                IContainer container = new Container();
                messageStream = new MessageStream(container);
                var subscription1 = new Subscription(typeof(TestIntMessage1), typeof(TestAddHandler1), (t) => FastActivator.CreateInstance(t));
                var subscription2 = new Subscription(typeof(TestIntMessage1), typeof(TestAddHandler2), (t) => FastActivator.CreateInstance(t));

                messages = new List<TransportMessage>();
                for (int i = 0; i < numberOfMessages; i++)
                {
                    messages.Add(new TransportMessage(new TestIntMessage1(i)));
                    messages.Add(new TransportMessage(new TestIntMessage2(i)));
                }
                messageStream.Subscribe(subscription1);
                messageStream.Subscribe(subscription2);

            };

        Because of = () =>
            {
                messageStream.Feed(messages);
            };

        It should_feed_all_handlers = () => HandleResult.Total.ShouldEqual(Enumerable.Range(0, numberOfMessages).Sum() * 2);

        static int numberOfMessages = 100;
        static MessageStream messageStream;
        static List<TransportMessage> messages;
    }
}