using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IMessageHandlerFactory
    {
        object CreateHandler(Type t);
    }
}