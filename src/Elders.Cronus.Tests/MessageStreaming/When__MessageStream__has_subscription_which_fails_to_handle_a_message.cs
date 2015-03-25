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
    public class When__MessageStream__has_subscription_which_fails_to_handle_a_message
    {
        Establish context = () =>
            {
                handlerFacotry = new CalculatorHandlerFactory();
                IContainer container = new Container();
                container.RegisterSingleton<IUnitOfWork>(() => new NoUnitOfWork());

                messageStream = new MessageProcessor("test", container);
                var subscription1 = new TestSubscription(typeof(CalculatorNumber2), new DefaultHandlerFactory(typeof(StandardCalculatorSubstractHandler), handlerFacotry.CreateInstance));
                var subscription2 = new TestSubscription(typeof(CalculatorNumber2), new DefaultHandlerFactory(typeof(CalculatorHandler_ThrowsException), handlerFacotry.CreateInstance));

                messages = new List<TransportMessage>();
                for (int i = 1; i < numberOfMessages + 1; i++)
                {
                    messages.Add(new TransportMessage(new Message(new CalculatorNumber2(i))));
                }
                messageStream.Subscribe(subscription1);
                messageStream.Subscribe(subscription2);
            };

        Because of = () =>
            {
                feedResult = messageStream.Feed(messages);
            };

        It should_feed_interested_handlers = () => handlerFacotry.State.Total.ShouldEqual(Enumerable.Range(1, numberOfMessages).Sum() * -1);

        It should_report_about_all_errors = () => feedResult.FailedMessages.ToList().ForEach(x => x.Errors.ForEach(e => e.Origin.Type.ShouldEqual("handler")));

        static IFeedResult feedResult;
        static int numberOfMessages = 3;
        static MessageProcessor messageStream;
        static List<TransportMessage> messages;
        static CalculatorHandlerFactory handlerFacotry;
    }
}
