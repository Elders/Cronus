using System;

namespace NMSD.Cronus.Core.Messaging
{
    public interface IConsumer<THandler>
    {
        void Stop();

        void Start(int numberOfWorkers);

        void RegisterHandler(Type messageType, Type handlerType, Func<Type, THandler> handlerFactory);
    }
}