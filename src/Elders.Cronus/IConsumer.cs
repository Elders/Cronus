using System.Threading.Tasks;

namespace Elders.Cronus
{
    public interface IConsumer<out T> where T : IMessageHandler
    {
        Task StartAsync();

        Task StopAsync();
    }
}
