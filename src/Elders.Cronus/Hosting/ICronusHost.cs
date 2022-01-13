using System;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus
{
    public interface ICronusHost : IDisposable
    {
        void Start();
        Task StartAsync();
        void Stop();
    }
}
