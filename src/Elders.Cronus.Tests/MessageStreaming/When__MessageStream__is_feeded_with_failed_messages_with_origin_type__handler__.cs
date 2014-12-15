using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.IocContainer;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.UnitOfWork;
using Machine.Specifications;

namespace Elders.Cronus.Tests.MessageStreaming
{

    [Subject("")]
    public class When__MessageStream__is_feeded_with_failed_messages_with_origin_type__handler__
    {
        Establish context = () =>
            {
                handlerFacotry = new CalculatorHandlerFactory();
                IContainer container = new Container();
                container.RegisterSingleton<IUnitOfWork>(() => new NoUnitOfWork());
                messageStream = new MessageProcessor(container);
                messages = new List<TransportMessage>();
                var subscription3 = new MessageProcessorSubscription(typeof(CalculatorNumber2), typeof(StandardCalculatorSubstractHandler), handlerFacotry.CreateInstance);
                var subscription4 = new MessageProcessorSubscription(typeof(CalculatorNumber2), typeof(StandardCalculatorAddHandler), handlerFacotry.CreateInstance);

                messageStream.Subscribe(subscription3);
                messageStream.Subscribe(subscription4);
                for (int i = 1; i < numberOfMessages + 1; i++)
                {
                    var normalMessage = new TransportMessage(new CalculatorNumber2(i));
                    var failedMessage = new TransportMessage(normalMessage, new FeedError() { Origin = new ErrorOrigin(subscription4.Id, "handler") });
                    failedMessage.Age = 2;
                    messages.Add(failedMessage);
                }
            };

        Because of = () =>
            {
                secondFeedResult = messageStream.Feed(messages);
            };

        It should_feed_interested_handlers = () => handlerFacotry.State.Total.ShouldEqual(Enumerable.Range(1, numberOfMessages).Sum());

        It should_report_about_all_successes = () => secondFeedResult.SuccessfulMessages.Count().ShouldEqual(numberOfMessages);

        static IFeedResult firstFeedResult;
        static IFeedResult secondFeedResult;
        static int numberOfMessages = 3;
        static MessageProcessor messageStream;
        static List<TransportMessage> messages;
        static CalculatorHandlerFactory handlerFacotry;
    }
}