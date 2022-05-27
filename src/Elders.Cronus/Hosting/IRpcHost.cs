using System;

namespace Elders.Cronus.Hosting
{
    public interface IRpcHost : IDisposable
    {
        void Start();
        void Stop();
    }
}
