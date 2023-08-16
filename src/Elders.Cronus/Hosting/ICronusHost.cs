using System;
using System.Threading.Tasks;

namespace Elders.Cronus
{
    public interface ICronusHost : IDisposable
    {
        Task StartAsync();
        Task StopAsync();
    }
}
