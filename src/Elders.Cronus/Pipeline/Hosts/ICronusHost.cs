using System;

namespace Elders.Cronus.Pipeline.Hosts
{
    public interface ICronusHost : IDisposable
    {
        bool Start();
        bool Stop();
    }
}