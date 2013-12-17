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
using NMSD.Cronus.Core.UnitOfWork;

namespace NMSD.Cronus.Core.Commanding
{
    public class RabbitCommandConsumer : RabbitConsumer<ICommand, IMessageHandler>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RabbitCommandConsumer));
        private Plumber plumber;
        private List<WorkPool> pools;
        string commandsQueueName;
        private readonly ProtoregSerializer serialiser;

        public RabbitCommandConsumer(ProtoregSerializer serialiser)
        {
            this.serialiser = serialiser;
            queueFactory = new CommonQueueFactory();
        }

        public override void Start(int numberOfWorkers)
        {
            pools = new List<WorkPool>();
            plumber = new Plumber();

            //  Think about this
            queueFactory.Compile(plumber.RabbitConnection);
            //  Think about this


            foreach (var queueInfo in queueFactory.Headers)
            {
                var pool = new WorkPool("defaultPool", numberOfWorkers);
                for (int i = 0; i < numberOfWorkers; i++)
                {
                    pool.AddWork(new ConsumerWork(this, queueInfo.Key));
                }
                pools.Add(pool);
                pool.StartCrawlers();
            }
        }

        public override void Stop()
        {
            foreach (WorkPool pool in pools)
            {
                pool.Stop();
            }
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
                            for (int i = 0; i < 1000; i++)
                            {
                                StartNewUnitOfWork();
                                try
                                {
                                    var rawMessage = newQueueingBasicConsumer.Queue.Dequeue();

                                    ICommand command;
                                    using (var stream = new MemoryStream(rawMessage.Body))
                                    {
                                        command = consumer.serialiser.Deserialize(stream) as ICommand;
                                    }

                                    if (consumer.Handle(command, unitOfWork))
                                        channel.BasicAck(rawMessage.DeliveryTag, false);
                                }
                                catch (EndOfStreamException)
                                {
                                    ScheduledStart = DateTime.UtcNow.AddMilliseconds(1000);
                                    break;
                                }
                                EndUnitOfWork();
                            }
                        }



                    }
                }
                catch (OperationInterruptedException ex)
                {

                    ScheduledStart = DateTime.UtcNow.AddMilliseconds(1000);

                }
            }

            public IUnitOfWorkPerBatch unitOfWork;
            public void StartNewUnitOfWork()
            {
                unitOfWork = consumer.UnitOfWorkFactory.NewBatch();
                if (unitOfWork != null)
                {
                    unitOfWork.Begin();
                }
            }
            public void EndUnitOfWork()
            {
                if (unitOfWork != null)
                {
                    unitOfWork.Commit();
                    unitOfWork.Dispose();
                    unitOfWork = null;
                }
            }
        }


    }
}