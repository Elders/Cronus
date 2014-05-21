using System;
using System.Collections.Generic;
using System.IO;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Multithreading.Work;
using Elders.Protoreg;

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

        private IConsumer<TContract> consumer;

        private readonly IEndpoint endpoint;

        volatile bool isWorking;

        private readonly ProtoregSerializer serializer;

        private readonly MessageThreshold messageThreshold;

        public PipelineConsumerWork(IConsumer<TContract> consumer, IEndpoint endpoint, ProtoregSerializer serializer, MessageThreshold messageThreshold)
        {
            this.endpoint = endpoint;
            this.consumer = consumer;
            this.serializer = serializer;
            this.messageThreshold = messageThreshold ?? new MessageThreshold();
        }

        public DateTime ScheduledStart { get; set; }

        private bool TryPullMessageFromEndpoint(out EndpointMessage rawMessage)
        {
            rawMessage = null;
            if (messageThreshold.Delay <= 0)
                rawMessage = endpoint.BlockDequeue();
            else
                endpoint.BlockDequeue(messageThreshold.Delay, out rawMessage);
            return rawMessage != null;
        }

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
                    for (int i = 0; i < messageThreshold.Size; i++)
                    {
                        EndpointMessage rawMessage = null;
                        if (!TryPullMessageFromEndpoint(out rawMessage))
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
