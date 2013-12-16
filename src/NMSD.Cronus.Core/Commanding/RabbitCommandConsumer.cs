using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.Multithreading.Work;
using NSMD.Cronus.RabbitMQ;
using Protoreg;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace NMSD.Cronus.Core.Commanding
{
    public class RabbitCommandConsumer : RabbitConsumer<ICommand, IMessageHandler>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RabbitCommandConsumer));
        private Plumber plumber;
        private WorkPool pool;
        string commandsQueueName;
        private readonly ProtoregSerializer serialiser;

        public RabbitCommandConsumer(ProtoregSerializer serialiser)
        {
            this.serialiser = serialiser;
            queueFactory = new CommonQueueFactory();
        }

        public override void Start(int numberOfWorkers)
        {
            plumber = new Plumber();

            //  Think about this
            queueFactory.Compile(plumber.RabbitConnection);
            //  Think about this

            pool = new WorkPool("defaultPool", numberOfWorkers);
            foreach (var queueInfo in queueFactory.Headers)
            {
                pool.AddWork(new ConsumerWork(this, queueInfo.Key));
            }

            pool.StartCrawlers();
        }

        public override void Stop()
        {
            pool.Stop();
        }

        private class ConsumerWork : IWork
        {
            private RabbitCommandConsumer consumer;
            private readonly string endpointName;

            public ConsumerWork(RabbitCommandConsumer consumer, string endpointName)
            {
                this.endpointName = endpointName;
                this.consumer = consumer;
            }

            public DateTime ScheduledStart { get; set; }

            public void Start()
            {
                try
                {
                    using (var channel = consumer.plumber.RabbitConnection.CreateModel())
                    {
                        QueueingBasicConsumer newQueueingBasicConsumer = new QueueingBasicConsumer();
                        channel.BasicConsume(endpointName, false, newQueueingBasicConsumer);

                        while (true)
                        {
                            try
                            {
                                var rawMessage = newQueueingBasicConsumer.Queue.Dequeue();

                                ICommand command;
                                using (var stream = new MemoryStream(rawMessage.Body))
                                {
                                    command = consumer.serialiser.Deserialize(stream) as ICommand;
                                }

                                if (consumer.Handle(command))
                                    channel.BasicAck(rawMessage.DeliveryTag, false);
                            }
                            catch (EndOfStreamException)
                            {
                                ScheduledStart = DateTime.UtcNow.AddMilliseconds(1000);
                                break;
                            }
                        }
                    }
                }
                catch (OperationInterruptedException ex)
                {

                    ScheduledStart = DateTime.UtcNow.AddMilliseconds(1000);

                }
            }
        }
    }
}