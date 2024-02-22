using System;
using System.Threading.Tasks;

namespace Elders.Cronus.Hosting;

public interface IRpcHost : IDisposable
{
    Task StartAsync();
    Task StopAsync();
}
