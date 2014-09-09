using System;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IHaveConsumer<TContract> where TContract : IMessage
    {
        Lazy<IConsumer<TContract>> Consumer { get; set; }
    }

}