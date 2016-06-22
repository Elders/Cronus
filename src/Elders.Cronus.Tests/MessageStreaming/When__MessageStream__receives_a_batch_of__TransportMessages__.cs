using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Tests.TestModel;
using Machine.Specifications;
using Elders.Cronus.MessageProcessingMiddleware;

namespace Elders.Cronus.Tests.MessageStreaming
{
    [Subject("")]
    public class When__MessageStream__receives_a_batch_of__TransportMessages__
    {
        Establish context = () =>
            {
                handlerFacotry = new CalculatorHandlerFactory();

                var messageHandlerMiddleware = new MessageHandlerMiddleware(handlerFacotry);
                var messageSubscriptionMiddleware = new MessageSubscriptionsMiddleware();
                messageHandlerMiddleware.ActualHandle = new DynamicMessageHandle();
                messageStream = new CronusMessageProcessorMiddleware("test", messageSubscriptionMiddleware);

                var subscription1 = new TestSubscriber(typeof(CalculatorNumber1), typeof(StandardCalculatorAddHandler), messageHandlerMiddleware);
                var subscription2 = new TestSubscriber(typeof(CalculatorNumber1), typeof(ScientificCalculatorHandler), messageHandlerMiddleware);

                messages = new List<CronusMessage>();
                for (int i = 1; i < numberOfMessages + 1; i++)
                {
                    messages.Add(new CronusMessage(new Message(new CalculatorNumber1(i))));
                    messages.Add(new CronusMessage(new Message(new CalculatorNumber2(i))));
                }
                messageSubscriptionMiddleware.Subscribe(subscription1);
                messageSubscriptionMiddleware.Subscribe(subscription2);

            };

        Because of = () =>
            {
                feedResult = messageStream.Run(messages);
            };

        It should_feed_all_handlers = () => handlerFacotry.State.Total.ShouldEqual(Enumerable.Range(1, numberOfMessages).Sum() * 2);

        It should_report_about_all_successes = () => feedResult.SuccessfulMessages.Count().ShouldEqual(numberOfMessages);

        static IFeedResult feedResult;
        static int numberOfMessages = 100;
        static CronusMessageProcessorMiddleware messageStream;
        static List<CronusMessage> messages;
        static CalculatorHandlerFactory handlerFacotry;
    }
}
