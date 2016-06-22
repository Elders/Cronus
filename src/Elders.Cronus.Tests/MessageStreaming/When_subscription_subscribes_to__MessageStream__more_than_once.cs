using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Tests.TestModel;
using Machine.Specifications;
using Elders.Cronus.MessageProcessingMiddleware;

namespace Elders.Cronus.Tests.MessageStreaming
{
    [Subject("")]
    public class When_subscription_subscribes_to__MessageStream__more_than_once
    {
        Establish context = () =>
            {
                handlerFacotry = new CalculatorHandlerFactory();

                var messageHandlerMiddleware = new MessageHandlerMiddleware(handlerFacotry);
                var messageSubscriptionMiddleware = new MessageSubscriptionsMiddleware();
                messageHandlerMiddleware.ActualHandle = new DynamicMessageHandle();
                messageStream = new CronusMessageProcessorMiddleware("test", messageSubscriptionMiddleware);


                var subscription1 = new TestSubscriber(typeof(CalculatorNumber1), typeof(StandardCalculatorAddHandler), messageHandlerMiddleware);

                messages = new List<CronusMessage>();
                for (int i = 1; i < numberOfMessages + 1; i++)
                {
                    messages.Add(new CronusMessage(new Message(new CalculatorNumber1(i))));
                }
                messageSubscriptionMiddleware.Subscribe(subscription1);
                messageSubscriptionMiddleware.Subscribe(subscription1);
                messageSubscriptionMiddleware.Subscribe(subscription1);
                messageSubscriptionMiddleware.Subscribe(subscription1);
            };

        Because of = () =>
            {
                feedResult = messageStream.Run(messages);
            };

        It should_accept_only_the_first_subscription = () => handlerFacotry.State.Total.ShouldEqual(Enumerable.Range(1, numberOfMessages).Sum());

        It should_report_about_all_successes = () => feedResult.SuccessfulMessages.Count().ShouldEqual(numberOfMessages);

        static IFeedResult feedResult;
        static int numberOfMessages = 2;
        static CronusMessageProcessorMiddleware messageStream;
        static List<CronusMessage> messages;
        static CalculatorHandlerFactory handlerFacotry;
    }
}
