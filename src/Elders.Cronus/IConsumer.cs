using System.Threading.Tasks;

namespace Elders.Cronus
{
    public interface IConsumer<out T> where T : IMessageHandler
    {
        void Start();
        Task StartAsync();
        void Stop();
    }

    public class EmptyConsumer<T> : IConsumer<T> where T : IMessageHandler
    {
        // Adds tracing once we have the logger in the IOC
        //private readonly ILogger<EmptyConsumer<T>> log;

        //public EmptyConsumer(ILogger<EmptyConsumer<T>> log)
        //{
        //    this.log = log;
        //}

        //public void Start() => log.LogTrace("Doing nothing."); 

        //public void Stop() => log.LogTrace("Doing nothing.");
        public void Start()
        {

        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }

        public void Stop()
        {

        }
    }
}
