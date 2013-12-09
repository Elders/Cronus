using System;
using System.IO;
using Cronus.Core.Eventing;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.Multithreading.Work;
using NSMD.Cronus.RabbitMQ;
using Protoreg;
using RabbitMQ.Client;

namespace NMSD.Cronus.Core.Eventing
{
    public class RabbitEventConsumer : RabbitConsumer<IEvent, IMessageHandler>
    {
        private WorkPool pool;

        private readonly ProtoregSerializer serialiser;

        public RabbitEventConsumer(ProtoregSerializer serialiser)
        {
            this.serialiser = serialiser;
            queueFactory = new QueuePerHandlerFactory();
        }

        public void Start(int numberOfWorkers)
        {
            Plumber plumber = new Plumber();

            //  Think about this
            queueFactory.Compile(plumber.RabbitConnection);
            // Think about this

            pool = new WorkPool("defaultPool", numberOfWorkers);
            foreach (var queueInfo in queueFactory.Headers)
            {
                pool.AddWork(new ConsumerWork(this, plumber.RabbitConnection, queueInfo.Key));
            }

            pool.StartCrawlers();
        }

        public void Stop()
        {
            pool.Stop();
        }


        private class ConsumerWork : IWork
        {
            private RabbitEventConsumer consumer;
            private readonly string queueName;
            private readonly IConnection rabbitConnection;

            public ConsumerWork(RabbitEventConsumer consumer, IConnection rabbitConnection, string queueName)
            {
                this.queueName = queueName;
                this.rabbitConnection = rabbitConnection;
                this.consumer = consumer;
            }

            public DateTime ScheduledStart { get; set; }

            public void Start()
            {
                using (var channel = rabbitConnection.CreateModel())//ConnectionClosedException??
                {
                    QueueingBasicConsumer newQueueingBasicConsumer = new QueueingBasicConsumer();
                    channel.BasicConsume(queueName, false, newQueueingBasicConsumer);
                    while (true)
                    {
                        try
                        {
                            var rawMessage = newQueueingBasicConsumer.Queue.Dequeue();
                            IEvent message;
                            if (rawMessage != null)
                            {
                                using (var stream = new MemoryStream(rawMessage.Body))
                                {
                                    message = consumer.serialiser.Deserialize(stream) as IEvent;
                                }

                                if (consumer.Handle(message))
                                    channel.BasicAck(rawMessage.DeliveryTag, false);
                            }
                        }
                        catch (EndOfStreamException)
                        {
                            ScheduledStart = DateTime.UtcNow.AddMilliseconds(1000);
                            break;
                        }
                    }

                }
            }
        }
    }
}