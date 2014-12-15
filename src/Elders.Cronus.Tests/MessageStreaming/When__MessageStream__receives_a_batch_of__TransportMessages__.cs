using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.IocContainer;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.UnitOfWork;
using Machine.Specifications;

namespace Elders.Cronus.Tests.MessageStreaming
{
    [Subject("")]
    public class When__MessageStream__receives_a_batch_of__TransportMessages__
    {
        Establish context = () =>
            {
                handlerFacotry = new CalculatorHandlerFactory();
                IContainer container = new Container();
                container.RegisterSingleton<IUnitOfWork>(() => new NoUnitOfWork());

                messageStream = new MessageProcessor(container);
                var subscription1 = new MessageProcessorSubscription(typeof(CalculatorNumber1), typeof(StandardCalculatorAddHandler), handlerFacotry.CreateInstance);
                var subscription2 = new MessageProcessorSubscription(typeof(CalculatorNumber1), typeof(ScientificCalculatorHandler), handlerFacotry.CreateInstance);

                messages = new List<TransportMessage>();
                for (int i = 1; i < numberOfMessages + 1; i++)
                {
                    messages.Add(new TransportMessage(new CalculatorNumber1(i)));
                    messages.Add(new TransportMessage(new CalculatorNumber2(i)));
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