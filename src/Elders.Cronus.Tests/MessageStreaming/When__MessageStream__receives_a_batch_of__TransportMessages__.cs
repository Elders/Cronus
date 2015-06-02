using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Tests.TestModel;
using Machine.Specifications;

namespace Elders.Cronus.Tests.MessageStreaming
{
    [Subject("")]
    public class When__MessageStream__receives_a_batch_of__TransportMessages__
    {
        Establish context = () =>
            {
                handlerFacotry = new CalculatorHandlerFactory();

                messageStream = new MessageProcessor("test");
                var subscription1 = new TestSubscription(typeof(CalculatorNumber1), new DefaultHandlerFactory(typeof(StandardCalculatorAddHandler), handlerFacotry.CreateInstance));
                var subscription2 = new TestSubscription(typeof(CalculatorNumber1), new DefaultHandlerFactory(typeof(ScientificCalculatorHandler), handlerFacotry.CreateInstance));

                messages = new List<TransportMessage>();
                for (int i = 1; i < numberOfMessages + 1; i++)
                {
                    messages.Add(new TransportMessage(new Message(new CalculatorNumber1(i))));
                    messages.Add(new TransportMessage(new Message(new CalculatorNumber2(i))));
                }
                messageStream.Subscribe(subscription1);
                messageStream.Subscribe(subscription2);

            };

        Because of = () =>
            {
                feedResult = messageStream.Feed(messages);
            };

        It should_feed_all_handlers = () => handlerFacotry.State.Total.ShouldEqual(Enumerable.Range(1, numberOfMessages).Sum() * 2);

        It should_report_about_all_successes = () => feedResult.SuccessfulMessages.Count().ShouldEqual(numberOfMessages);

        static IFeedResult feedResult;
        static int numberOfMessages = 100;
        static MessageProcessor messageStream;
        static List<TransportMessage> messages;
        static CalculatorHandlerFactory handlerFacotry;
    }
}
