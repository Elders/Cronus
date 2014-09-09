using System;
using Elders.Cronus.Pipeline.Transport;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IHaveTransport<TTransport> where TTransport : ITransport
    {
        Lazy<TTransport> Transport { get; set; }
    }
}