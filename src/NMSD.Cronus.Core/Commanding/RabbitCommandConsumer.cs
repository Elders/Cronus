using System;
using System.Collections.Generic;
using System.IO;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.Multithreading.Work;
using NSMD.Cronus.RabbitMQ;
using Protoreg;
using RabbitMQ.Client;

namespace NMSD.Cronus.Core.Commanding
{
    public class RabbitCommandConsumer : IConsumer<IMessageHandler>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RabbitCommandConsumer));

        private string commandsQueueName;

        private Dictionary<Type, List<Func<ICommand, bool>>> handlers = new Dictionary<Type, List<Func<ICommand, bool>>>();

        private IDictionary<string, object> headers;

        private string pipeName;

        private WorkPool pool;

        private readonly ProtoregSerializer serialiser;

        public RabbitCommandConsumer(ProtoregSerializer serialiser)
        {
            this.serialiser = serialiser;
            headers = new Dictionary<string, object>();
            headers.Add("x-match", "any");
        }

        public void RegisterHandler(Type eventType, Type eventHandlerType, Func<Type, IMessageHandler> handlerFactory)
        {
            if (!handlers.ContainsKey(eventType))
                handlers[eventType] = new List<Func<ICommand, bool>>();

            handlers[eventType].Add(x => Handle(x, eventHandlerType, handlerFactory));

            AddMessageContractToQueueHeaders(eventType, eventHandlerType);
        }

        public void Start(int numberOfWorkers)
        {
            var plumber = new Plumber("192.168.16.69");

            //  Think about this
            CreateEndpoint(plumber.RabbitConnection, pipeName, commandsQueueName, headers);
            //  Think about this

            pool = new WorkPool("defaultPool", numberOfWorkers);
            for (int i = 0; i < numberOfWorkers; i++)
            {
                pool.AddWork(new ConsumerWork(this, plumber.RabbitConnection, commandsQueueName));
            }

            pool.StartCrawlers();
        }

        public void Stop()
        {
            pool.Stop();
        }

        private void AddMessageContractToQueueHeaders(Type messageType, Type messageHandlerType)
        {
            pipeName = MessagingHelper.GetPipelineName(messageType);

            const string queueSuffix = "Commands";
            commandsQueueName = String.Format("{0}.{1}", MessagingHelper.GetBoundedContextNamespace(messageHandlerType), queueSuffix);
            headers.Add(MessagingHelper.GetMessageId(messageType), String.Empty);
        }

        private void CreateEndpoint(IConnection rabbitConnection, string pipelineName, string endpointName, IDictionary<string, object> headers)
        {
            using (var channel = rabbitConnection.CreateModel())
            {
                channel.ExchangeDeclare(pipelineName, "headers");
                channel.QueueDeclare(endpointName, true, false, false, headers);
                channel.QueueBind(endpointName, pipelineName, String.Empty, headers);
            }
        }

        private bool Handle(ICommand message)
        {
            List<Func<ICommand, bool>> availableHandlers;
            if (handlers.TryGetValue(message.GetType(), out availableHandlers))
            {
                foreach (var handleMethod in availableHandlers)
                {
                    var result = handleMethod(message);
                    if (result == false)
                        return result;
                }
                log.Info("HANDLE => " + message.ToString());
            }
            return true;
        }

        private bool Handle(ICommand message, Type eventHandlerType, Func<Type, IMessageHandler> handlerFactory)
        {
            dynamic handler = null;
            try
            {
                handler = handlerFactory(eventHandlerType);
                handler.Handle((dynamic)message);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private class ConsumerWork : IWork
        {
            private RabbitCommandConsumer consumer;
            private readonly string endpointName;
            private readonly IConnection rabbitConnection;
            IModel channel;

            public ConsumerWork(RabbitCommandConsumer consumer, IConnection rabbitConnection, string endpointName)
            {
                this.endpointName = endpointName;
                this.rabbitConnection = rabbitConnection;
                this.consumer = consumer;
                // channel = rabbitConnection.CreateModel();
            }

            public DateTime ScheduledStart { get; set; }

            public void Start()
            {
                using (var channel = rabbitConnection.CreateModel())//ConnectionClosedException??
                {

                    QueueingBasicConsumer newQueueingBasicConsumer = new QueueingBasicConsumer();
                    channel.BasicConsume(endpointName, false, newQueueingBasicConsumer);
                    while (true)
                    {
                        var rawMessage = newQueueingBasicConsumer.Queue.Dequeue();
                        ICommand command;
                        if (rawMessage != null)
                        {
                            using (var stream = new MemoryStream(rawMessage.Body))
                            {
                                command = consumer.serialiser.Deserialize(stream) as ICommand;
                            }
                            if (consumer.Handle(command))
                                channel.BasicAck(rawMessage.DeliveryTag, false);
                        }
                        else
                        {
                            //Console.WriteLine("null");
                            //ScheduledStart = DateTime.UtcNow.AddMilliseconds(500);
                        }
                    }



                }

            }
        }
    }

    public class RabbitCommandPublisher : RabbitPublisher<ICommand>
    {
        public RabbitCommandPublisher(ProtoregSerializer serialiser)
            : base(serialiser)
        {

        }
    }
}
