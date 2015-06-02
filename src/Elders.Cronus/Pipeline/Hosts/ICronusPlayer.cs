using System;

namespace Elders.Cronus.Pipeline.Hosts
{
    public interface ICronusPlayer : IDisposable
    {
        bool Replay();
        bool Stop();
    }
}
