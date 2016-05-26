using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Tests.TestModel;
using Machine.Specifications;
using Elders.Cronus.MessageProcessingMiddleware;

namespace Elders.Cronus.Tests.MessageStreaming
{
    [Subject("")]
    public class When__MessageStream__has_subscription_which_fails_to_handle_a_message
    {
        Establish context = () =>
            {
                handlerFacotry = new CalculatorHandlerFactory();

                var messageHandlerMiddleware = new MessageHandlerMiddleware(handlerFacotry);
                var messageSubscriptionMiddleware = new MessageSubscriptionsMiddleware();
                messageStream = new CronusMessageProcessorMiddleware("test", messageSubscriptionMiddleware);

                var subscription1 = new TestSubscriber(typeof(CalculatorNumber2), typeof(StandardCalculatorSubstractHandler), messageHandlerMiddleware);
                var subscription2 = new TestSubscriber(typeof(CalculatorNumber2), typeof(CalculatorHandler_ThrowsException), messageHandlerMiddleware);

                messages = new List<TransportMessage>();
                for (int i = 1; i < numberOfMessages + 1; i++)
                {
                    messages.Add(new TransportMessage(new Message(new CalculatorNumber2(i))));
                }
                messageSubscriptionMiddleware.Subscribe(subscription1);
                messageSubscriptionMiddleware.Subscribe(subscription2);
            };

        Because of = () =>
            {
                feedResult = messageStream.Run(messages);
            };

        It should_feed_interested_handlers = () => handlerFacotry.State.Total.ShouldEqual(Enumerable.Range(1, numberOfMessages).Sum() * -1);

        It should_report_about_all_errors = () => feedResult.FailedMessages.ToList().ForEach(x => x.Errors.ForEach(e => e.Origin.Type.ShouldEqual("handler")));

        static IFeedResult feedResult;
        static int numberOfMessages = 3;
        static CronusMessageProcessorMiddleware messageStream;
        static List<TransportMessage> messages;
        static CalculatorHandlerFactory handlerFacotry;
    }
}
