namespace Elders.Cronus
{
    public abstract class StartupBase<T>
    {
        private readonly IConsumer<T> consumer;

        public StartupBase(IConsumer<T> consumer)
        {
            this.consumer = consumer;
        }

        public void Start() => consumer.Start();

        public void Stop() => consumer.Stop();
    }
}
