//using System;
//using System.Collections.Generic;
//using Elders.Cronus.DomainModelling;

//namespace Elders.Cronus.Pipeline
//{
//    public interface IConsumer : ITransportIMessage
//    {
//        bool Consume(List<IMessage> messages);
//        IEnumerable<Type> GetRegisteredHandlers { get; }
//        IEndpointConsumerErrorStrategy ErrorStrategy { get; }
//        IEndpointConsumerSuccessStrategy SuccessStrategy { get; }
//    }

//    public interface IConsumer<T> : IConsumer where T : IMessage
//    {

//    }
//}