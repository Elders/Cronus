using System;
using System.Collections.Generic;
using System.IO;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Pipeline.CircuitBreaker;
using Elders.Cronus.Serializer;
using Elders.Multithreading.Scheduler;

namespace Elders.Cronus.Pipeline
{
    public sealed class MessageThreshold
    {
        public MessageThreshold() : this(100, 30) { }

        /// <summary>
        /// If the size is > 1 and the delay is 0 could be dangerous. Use only in special cases and you should be familiar with the PipelineConsumerWork code.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="delay"></param>
        public MessageThreshold(uint size, uint delay)
        {
            if (size == 0) throw new ArgumentException("The size cannot be 0", "size");

            this.Size = size;
            this.Delay = delay;
        }

        public uint Size { get; private set; }
        public uint Delay { get; private set; }
    }

    public class PipelineConsumerWork<TContract> : IWork where TContract : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(PipelineConsumerWork<TContract>));

        private IMessageProcessor<TContract> processor;

        private readonly IEndpoint endpoint;

        volatile bool isWorking;

        private readonly ISerializer serializer;

        private readonly MessageThreshold messageThreshold;

        private readonly IEndpointCircuitBreaker circuitBreaker;

        public PipelineConsumerWork(IMessageProcessor<TContract> processor, IEndpoint endpoint, ISerializer serializer, MessageThreshold messageThreshold, IEndpointCircuitBreaker circuitBreaker)
        {
            this.endpoint = endpoint;
            this.processor = processor;
            this.processor = processor;
            this.serializer = serializer;
            this.messageThreshold = messageThreshold ?? new MessageThreshold();
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
                    List<TransportMessage> transportMessages = new List<TransportMessage>();
                    for (int i = 0; i < messageThreshold.Size; i++)
                    {
                        EndpointMessage rawMessage = null;
                        if (!endpoint.BlockDequeue(messageThreshold.Delay, out rawMessage))
                            break;

                        TransportMessage transportMessage = (TransportMessage)serializer.DeserializeFromBytes(rawMessage.Body);

                        transportMessages.Add(transportMessage);
                        rawMessages.Add(rawMessage);
                    }
                    if (transportMessages.Count == 0)
                        continue;
                    var result = processor.Handle(transportMessages);
                    circuitBreaker.PostConsume(result);
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
            endpoint.Close();
        }

    }
}
