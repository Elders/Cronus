//using System;
//using System.Collections.Generic;

//namespace Elders.Cronus.MessageProcessing
//{
//    internal class CronusPublisher<TMessage> : IPublisher<TMessage> where TMessage : IMessage
//    {
//        readonly IPublisher<TMessage> publisher;
//        readonly CronusMessage cronusMessage;

//        public CronusPublisher(IPublisher<TMessage> publisher, CronusMessage cronusMessage)
//        {
//            this.cronusMessage = cronusMessage;
//            this.publisher = publisher;
//        }

//        public bool Publish(TMessage message, Dictionary<string, string> messageHeaders = null)
//        {
//            messageHeaders = messageHeaders ?? new Dictionary<string, string>();
//            AddTrackHeaders(messageHeaders, cronusMessage);
//            return publisher.Publish(message, messageHeaders);
//        }

//        public bool Publish(TMessage message, TimeSpan publishAfter, Dictionary<string, string> messageHeaders = null)
//        {
//            messageHeaders = messageHeaders ?? new Dictionary<string, string>();
//            AddTrackHeaders(messageHeaders, cronusMessage);
//            return publisher.Publish(message, publishAfter, messageHeaders);
//        }

//        public bool Publish(TMessage message, DateTime publishAt, Dictionary<string, string> messageHeaders = null)
//        {
//            messageHeaders = messageHeaders ?? new Dictionary<string, string>();
//            AddTrackHeaders(messageHeaders, cronusMessage);
//            return publisher.Publish(message, publishAt, messageHeaders);
//        }

//        public bool Publish(ReadOnlyMemory<byte> messageRaw, Type messageRawType, Dictionary<string, string> messageHeaders = null)
//        {
//            messageHeaders = messageHeaders ?? new Dictionary<string, string>();
//            AddTrackHeaders(messageHeaders, cronusMessage);

//            return publisher.Publish(messageRaw, messageRawType, messageHeaders);
//        }

//        Dictionary<string, string> AddTrackHeaders(Dictionary<string, string> messageHeaders, CronusMessage triggeredBy)
//        {
//            messageHeaders.Add(MessageHeader.CausationId, triggeredBy.MessageId);
//            messageHeaders.Add(MessageHeader.CorelationId, triggeredBy.CorelationId);

//            return messageHeaders;
//        }
//    }
//}
