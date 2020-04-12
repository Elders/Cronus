using Microsoft.Extensions.Logging;

namespace Elders.Cronus
{
    public interface IConsumer<T>
    {
        void Start();

        void Stop();
    }

    public class EmptyConsumer<T> : IConsumer<T>
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

        public void Stop()
        {

        }
    }
}
