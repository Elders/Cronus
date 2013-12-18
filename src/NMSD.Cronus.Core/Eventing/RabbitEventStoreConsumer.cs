using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.EventStoreEngine;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.Multithreading.Work;
using NSMD.Cronus.RabbitMQ;
using NMSD.Protoreg;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace NMSD.Cronus.Core.Eventing
{
    public class RabbitEventStoreConsumer : RabbitConsumer<MessageCommit, IMessageHandler>
    {
        RabbitEventPublisher eventPublisher;

        ProtoEventStore eventStore;

        private Plumber plumber;
        private List<WorkPool> pools;

        private readonly ProtoregSerializer serialiser;

        public RabbitEventStoreConsumer(Assembly assembly, ProtoregSerializer serialiser, ProtoEventStore eventStore)
        {
            this.serialiser = serialiser;
            eventPublisher = new RabbitEventPublisher(serialiser);
            this.eventStore = eventStore;
            queueFactory = new EventStoreQueueFactory(assembly);
        }

        public override void Start(int numberOfWorkers)
        {
            plumber = new Plumber();

            //  Think about this
            queueFactory.Compile(plumber.RabbitConnection);
            // Think about this

            pools = new List<WorkPool>();
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
            private RabbitEventStoreConsumer consumer;
            private readonly string queueName;
            public ConsumerWork(RabbitEventStoreConsumer consumer, string queueName)
            {
                this.queueName = queueName;
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
                        channel.BasicConsume(queueName, false, newQueueingBasicConsumer);

                        List<BasicDeliverEventArgs> rawMessages = new List<BasicDeliverEventArgs>();
                        List<IEvent> eventsBatch = new List<IEvent>();
                        List<IAggregateRootState> statesBatch = new List<IAggregateRootState>();
                        var connection = consumer.eventStore.OpenConnection();
                        try
                        {
                            while (true)
                            {
                                rawMessages.Clear();
                                eventsBatch.Clear();
                                statesBatch.Clear();

                                try
                                {
                                    for (int i = 0; i < 100; i++)
                                    {
                                        BasicDeliverEventArgs rawMessage;
                                        if (!newQueueingBasicConsumer.Queue.Dequeue(30, out rawMessage))
                                            break;

                                        rawMessages.Add(rawMessage);
                                        MessageCommit message;
                                        using (var stream = new MemoryStream(rawMessage.Body))
                                        {
                                            message = consumer.serialiser.Deserialize(stream) as MessageCommit;
                                        }
                                        eventsBatch.AddRange(message.Events);
                                        statesBatch.Add(message.State);
                                    }

                                    if (eventsBatch.Count > 0)
                                    {
                                        consumer.eventStore.Persist(eventsBatch, connection);
                                        consumer.eventStore.TakeSnapshot(statesBatch, connection);

                                        foreach (var @event in eventsBatch)
                                        {
                                            consumer.eventPublisher.Publish(@event);
                                        }

                                        foreach (var rawMessage in rawMessages)
                                        {
                                            channel.BasicAck(rawMessage.DeliveryTag, false);
                                        }
                                        //channel.BasicAck(rawMessages.First().DeliveryTag, true);
                                    }
                                }
                                catch (EndOfStreamException)
                                {
                                    ScheduledStart = DateTime.UtcNow.AddMilliseconds(1000);
                                    break;
                                }
                                catch (AlreadyClosedException)
                                {
                                    ScheduledStart = DateTime.UtcNow.AddMilliseconds(1000);

                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            consumer.eventStore.CloseConnection(connection);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                    ScheduledStart = DateTime.UtcNow.AddMilliseconds(1000);
                }
            }

        }
    }
}