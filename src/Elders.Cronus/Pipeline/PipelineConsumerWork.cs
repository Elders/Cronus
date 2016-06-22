using System;
using System.Collections.Generic;
using Elders.Cronus.Pipeline.CircuitBreaker;
using Elders.Cronus.Serializer;
using Elders.Multithreading.Scheduler;
using Elders.Cronus.Logging;

namespace Elders.Cronus.Pipeline
{
    public class PipelineConsumerWork : IWork
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(PipelineConsumerWork));

        private IMessageProcessor processor;

        private readonly IEndpoint endpoint;

        volatile bool isWorking;

        private readonly ISerializer serializer;

        private readonly MessageThreshold messageThreshold;

        private readonly IEndpointCircuitBreaker circuitBreaker;

        public PipelineConsumerWork(IMessageProcessor processor, IEndpoint endpoint, ISerializer serializer, MessageThreshold messageThreshold, IEndpointCircuitBreaker circuitBreaker)
        {
            this.endpoint = endpoint;
            this.processor = processor;
            this.circuitBreaker = circuitBreaker;
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
                    List<CronusMessage> transportMessages = new List<CronusMessage>();
                    for (int i = 0; i < messageThreshold.Size; i++)
                    {
                        EndpointMessage rawMessage = null;
                        if (!endpoint.BlockDequeue(messageThreshold.Delay, out rawMessage))
                            break;

                        CronusMessage transportMessage = (CronusMessage)serializer.DeserializeFromBytes(rawMessage.Body);

                        transportMessages.Add(transportMessage);
                        rawMessages.Add(rawMessage);
                    }
                    if (transportMessages.Count == 0)
                        continue;

                    var result = processor.Run(transportMessages);
                    circuitBreaker.PostConsume(result);
                    endpoint.AcknowledgeAll();
                }
            }
            catch (EndpointClosedException ex)
            {
                log.WarnException("Endpoint Closed", ex);
            }
            catch (Exception ex)
            {
                log.ErrorException("Unexpected Exception.", ex);
            }
            finally
            {
                try
                {
                    endpoint.AcknowledgeAll();
                    endpoint.Close();
                }
                catch (EndpointClosedException ex)
                {
                    log.WarnException("Endpoint Closed", ex);
                }
                ScheduledStart = DateTime.UtcNow.AddMilliseconds(30);
            }
        }

        public void Stop()
        {
            isWorking = false;
            endpoint.Close();
        }
    }
}
