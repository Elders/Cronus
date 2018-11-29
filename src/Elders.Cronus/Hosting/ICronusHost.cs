using System;

namespace Elders.Cronus
{
    public interface ICronusHost : IDisposable
    {
        void Start();
        void Stop();
    }
}
