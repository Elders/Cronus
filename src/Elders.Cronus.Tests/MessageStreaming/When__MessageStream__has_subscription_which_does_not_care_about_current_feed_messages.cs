using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.IocContainer;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Tests.TestModel;
using Elders.Cronus.UnitOfWork;
using Machine.Specifications;

namespace Elders.Cronus.Tests.MessageStreaming
{
    [Subject("")]
    public class When__MessageStream__has_subscription_which_does_not_care_about_current_feed_messages
    {
        Establish context = () =>
            {
                handlerFacotry = new CalculatorHandlerFactory();
                IContainer container = new Container();
                container.RegisterSingleton<IUnitOfWork>(() => new NoUnitOfWork());

                messageStream = new MessageProcessor("test", container);
                var subscription1 = new TestSubscription(typeof(CalculatorNumber1), new DefaultHandlerFactory(typeof(StandardCalculatorAddHandler), handlerFacotry.CreateInstance));
                var subscription2 = new TestSubscription(typeof(CalculatorNumber1), new DefaultHandlerFactory(typeof(ScientificCalculatorHandler), handlerFacotry.CreateInstance));
                var subscription3 = new TestSubscription(typeof(CalculatorNumber2), new DefaultHandlerFactory(typeof(StandardCalculatorSubstractHandler), handlerFacotry.CreateInstance));

                messages = new List<TransportMessage>();
                for (int i = 1; i < numberOfMessages + 1; i++)
                {
                    messages.Add(new TransportMessage(new Message(new CalculatorNumber2(i))));
                }
                messageStream.Subscribe(subscription1);
                messageStream.Subscribe(subscription2);
                messageStream.Subscribe(subscription3);
            };

        Because of = () =>
            {
                feedResult = messageStream.Feed(messages);
            };

        It should_feed_only_interested_handlers = () => handlerFacotry.State.Total.ShouldEqual(Enumerable.Range(1, numberOfMessages).Sum() * -1);
        It should_report_about_all_successes = () => feedResult.SuccessfulMessages.Count().ShouldEqual(numberOfMessages);

        static IFeedResult feedResult;
        static int numberOfMessages = 100;
        static MessageProcessor messageStream;
        static List<TransportMessage> messages;
        static CalculatorHandlerFactory handlerFacotry;
    }

    [Subject("")]
    public class When__MessageStream_
    {
        Establish context = () =>
            {
                handlerFacotry = new CalculatorHandlerFactory();
                IContainer container = new Container();
                container.RegisterSingleton<IUnitOfWork>(() => new NoUnitOfWork());

                messageStream = new MessageProcessor("test", container);
                var subscription1 = new TestSubscription(typeof(CalculatorNumber1), new DefaultHandlerFactory(typeof(StandardCalculatorAddHandler), handlerFacotry.CreateInstance));
                var subscription2 = new TestSubscription(typeof(CalculatorNumber1), new DefaultHandlerFactory(typeof(ScientificCalculatorHandler), handlerFacotry.CreateInstance));
                var subscription3 = new TestSubscription(typeof(CalculatorNumber2), new DefaultHandlerFactory(typeof(StandardCalculatorSubstractHandler), handlerFacotry.CreateInstance));

                messages = new List<TransportMessage>();
                for (int i = 1; i < numberOfMessages + 1; i++)
                {
                    messages.Add(new TransportMessage(new Message(new CalculatorNumber2(i))));
                }
                messageStream.Subscribe(subscription1);
                messageStream.Subscribe(subscription2);
                messageStream.Subscribe(subscription3);
            };

        Because of = () =>
            {
                feedResult = messageStream.Feed(messages);
            };

        It should_feed_only_interested_handlers = () => handlerFacotry.State.Total.ShouldEqual(Enumerable.Range(1, numberOfMessages).Sum() * -1);

        It should_report_about_all_successes = () => feedResult.SuccessfulMessages.Count().ShouldEqual(numberOfMessages);

        static IFeedResult feedResult;
        static int numberOfMessages = 100;
        static MessageProcessor messageStream;
        static List<TransportMessage> messages;
        static CalculatorHandlerFactory handlerFacotry;
    }
}
