using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Tests.TestModel;
using Machine.Specifications;

namespace Elders.Cronus.Tests.MessageStreaming
{
    [Subject("")]
    public class When_subscription_subscribes_to__MessageStream__more_than_once
    {
        Establish context = () =>
            {
                handlerFacotry = new CalculatorHandlerFactory();
                messageStream = new MessageProcessor("test");
                var subscription1 = new TestSubscription(typeof(CalculatorNumber1), new DefaultHandlerFactory(typeof(StandardCalculatorAddHandler), handlerFacotry.CreateInstance));

                messages = new List<TransportMessage>();
                for (int i = 1; i < numberOfMessages + 1; i++)
                {
                    messages.Add(new TransportMessage(new Message(new CalculatorNumber1(i))));
                }
                messageStream.Subscribe(subscription1);
                messageStream.Subscribe(subscription1);
                messageStream.Subscribe(subscription1);
                messageStream.Subscribe(subscription1);
            };

        Because of = () =>
            {
                feedResult = messageStream.Feed(messages);
            };

        It should_accept_only_the_first_subscription = () => handlerFacotry.State.Total.ShouldEqual(Enumerable.Range(1, numberOfMessages).Sum());

        It should_report_about_all_successes = () => feedResult.SuccessfulMessages.Count().ShouldEqual(numberOfMessages);

        static IFeedResult feedResult;
        static int numberOfMessages = 2;
        static MessageProcessor messageStream;
        static List<TransportMessage> messages;
        static CalculatorHandlerFactory handlerFacotry;
    }
}
