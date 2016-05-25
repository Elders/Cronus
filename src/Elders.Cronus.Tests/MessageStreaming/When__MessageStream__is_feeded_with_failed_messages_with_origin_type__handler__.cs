using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Tests.TestModel;
using Machine.Specifications;
using Elders.Cronus.MessageProcessingMiddleware;

namespace Elders.Cronus.Tests.MessageStreaming
{
    [Subject("")]
    public class When__MessageStream__is_feeded_with_failed_messages_with_origin_type__handler__
    {
        Establish context = () =>
            {
                handlerFacotry = new CalculatorHandlerFactory();

                var messageHandlerMiddleware = new MessageHandlerMiddleware(handlerFacotry);
                var messageSubscriptionMiddleware = new MessageSubscriptionsMiddleware();
                messageStream = new CronusMessageProcessorMiddleware("test", messageSubscriptionMiddleware);

                messages = new List<TransportMessage>();
                var subscription3 = new TestSubscriber(typeof(CalculatorNumber2), typeof(StandardCalculatorSubstractHandler), messageHandlerMiddleware);
                var subscription4 = new TestSubscriber(typeof(CalculatorNumber2), typeof(StandardCalculatorAddHandler), messageHandlerMiddleware);

                messageSubscriptionMiddleware.Subscribe(subscription3);
                messageSubscriptionMiddleware.Subscribe(subscription4);
                for (int i = 1; i < numberOfMessages + 1; i++)
                {
                    var normalMessage = new TransportMessage(new Message(new CalculatorNumber2(i)));
                    var failedMessage = new TransportMessage(normalMessage, new FeedError() { Origin = new ErrorOrigin(subscription4.Id, "handler") });
                    failedMessage.Age = 2;
                    messages.Add(failedMessage);
                }
            };

        Because of = () =>
            {
                secondFeedResult = messageStream.Invoke(messages);
            };

        It should_feed_interested_handlers = () => handlerFacotry.State.Total.ShouldEqual(Enumerable.Range(1, numberOfMessages).Sum());

        It should_report_about_all_successes = () => secondFeedResult.SuccessfulMessages.Count().ShouldEqual(numberOfMessages);

        static IFeedResult secondFeedResult;
        static int numberOfMessages = 3;
        static CronusMessageProcessorMiddleware messageStream;
        static List<TransportMessage> messages;
        static CalculatorHandlerFactory handlerFacotry;
    }
}
