using System;
using System.Collections.Generic;
using Elders.Cronus.Serializer;
using Elders.Multithreading.Scheduler;
using Elders.Cronus.Logging;
using Elders.Cronus.Netflix;

namespace Elders.Cronus.Pipeline
{
    public class PipelineConsumerWork : IWork
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(PipelineConsumerWork));

        private SubscriptionMiddleware subscriptions;

        private readonly IEndpoint endpoint;

        volatile bool isWorking;

        private readonly ISerializer serializer;

        private readonly MessageThreshold messageThreshold;

        public PipelineConsumerWork(SubscriptionMiddleware subscriptions, IEndpoint endpoint, ISerializer serializer, MessageThreshold messageThreshold)
        {
            this.endpoint = endpoint;
            this.subscriptions = subscriptions;
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
                    for (int i = 0; i < messageThreshold.Size; i++)
                    {
                        EndpointMessage rawMessage = null;
                        if (!endpoint.BlockDequeue(messageThreshold.Delay, out rawMessage))
                            break;

                        CronusMessage transportMessage = (CronusMessage)serializer.DeserializeFromBytes(rawMessage.Body);
                        var subscribers = subscriptions.GetInterestedSubscribers(transportMessage);
                        foreach (var subscriber in subscribers)
                        {
                            subscriber.Process(transportMessage);
                        }
                        rawMessages.Add(rawMessage);
                    }

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
