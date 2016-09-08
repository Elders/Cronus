using System;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IMessageHandlerFactory
    {
        object CreateHandler(Type t);
    }
}