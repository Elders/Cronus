using System;
using System.IO;
using System.Runtime.Serialization;
using Cronus.Core.Eventing;
using NMSD.Cronus.Core.Cqrs;
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
        }

        public void Start(int numberOfWorkers)
        {
            Plumber plumber = new Plumber();

            //  Think about this
            foreach (var item in handlerQueues)
            {
                CreateEndpoint(plumber.RabbitConnection, pipeName, item.Key, item.Value);
            }
            // Think about this

            pool = new WorkPool("defaultPool", numberOfWorkers);
            foreach (var queueInfo in handlerQueues)
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

    public class RabbitSystemConsumer : RabbitConsumer<MessageCommit, IMessageHandler>
    {
        private WorkPool pool;

        private readonly ProtoregSerializer serialiser;
        RabbitEventPublisher eventPublisher;

        public RabbitSystemConsumer(ProtoregSerializer serialiser)
        {
            this.serialiser = serialiser;
            eventPublisher = new RabbitEventPublisher(serialiser);
        }

        public void Start(int numberOfWorkers)
        {
            Plumber plumber = new Plumber();

            //  Think about this
            foreach (var item in handlerQueues)
            {
                CreateEndpoint(plumber.RabbitConnection, pipeName, item.Key, item.Value);
            }
            // Think about this

            pool = new WorkPool("defaultPool", numberOfWorkers);
            foreach (var queueInfo in handlerQueues)
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
            private RabbitSystemConsumer consumer;
            private readonly string queueName;
            private readonly IConnection rabbitConnection;

            public ConsumerWork(RabbitSystemConsumer consumer, IConnection rabbitConnection, string queueName)
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
                        var rawMessage = newQueueingBasicConsumer.Queue.Dequeue();
                        MessageCommit message;
                        if (rawMessage != null)
                        {
                            using (var stream = new MemoryStream(rawMessage.Body))
                            {
                                message = consumer.serialiser.Deserialize(stream) as MessageCommit;
                            }
                            if (consumer.Handle(message))
                            {
                                foreach (var @event in message.Events)
                                {
                                    consumer.eventPublisher.Publish(@event);
                                }
                                channel.BasicAck(rawMessage.DeliveryTag, false);
                            }
                            else
                            {
                                throw new Exception("Resens command or something else ffs");
                            }
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

    public class RabbitSystemPublisher : RabbitPublisher<MessageCommit>
    {
        public RabbitSystemPublisher(ProtoregSerializer serialiser)
            : base(serialiser)
        {

        }
    }

    public class RabbitEventPublisher : RabbitPublisher<IEvent>
    {
        public RabbitEventPublisher(ProtoregSerializer serialiser)
            : base(serialiser)
        {

        }
    }
}
