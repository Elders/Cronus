using System;
using System.Collections.Generic;
using System.IO;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Multithreading.Work;
using Elders.Protoreg;

namespace Elders.Cronus.Pipeline
{
    public class PipelineConsumerWork<TContract> : IWork where TContract : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(PipelineConsumerWork<TContract>));

        private IConsumer<TContract> consumer;

        private readonly IEndpoint endpoint;

        volatile bool isWorking;

        private readonly ProtoregSerializer serializer;

        private readonly int batchSize;

        public PipelineConsumerWork(IConsumer<TContract> consumer, IEndpoint endpoint, ProtoregSerializer serializer, int batchSize)
        {
            this.endpoint = endpoint;
            this.consumer = consumer;
            this.serializer = serializer;
            this.batchSize = batchSize;
        }

        public DateTime ScheduledStart { get; set; }

        public void Start()
        {
            try
            {
                isWorking = true;
                endpoint.Open();
                while (isWorking)
                {
                    List<EndpointMessage> rawMessages = new List<EndpointMessage>();
                    List<TContract> messages = new List<TContract>();
                    for (int i = 0; i < batchSize; i++)
                    {
                        EndpointMessage rawMessage;
                        endpoint.BlockDequeue(30, out rawMessage);
                        if (rawMessage == null)
                            break;

                        TContract message;
                        using (var stream = new MemoryStream(rawMessage.Body))
                        {
                            message = (TContract)serializer.Deserialize(stream);
                        }

                        messages.Add(message);
                        rawMessages.Add(rawMessage);
                    }
                    if (messages.Count == 0)
                        continue;

                    consumer.Consume(messages);
                    endpoint.AcknowledgeAll();
                }
            }
            catch (EndpointClosedException ex)
            {
                log.Error("Endpoint Closed", ex);
            }
            catch (Exception ex)
            {
                log.Error("Unexpected Exception.", ex);
            }
            finally
            {
                endpoint.AcknowledgeAll();
                ScheduledStart = DateTime.UtcNow.AddMilliseconds(30);
                endpoint.Close();
            }
        }

        public void Stop()
        {
            isWorking = false;
        }

    }
}
