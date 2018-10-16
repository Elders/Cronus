namespace Elders.Cronus.Pipeline
{
    public interface IConsumer<T>
    {
        void Start();

        void Stop();
    }

    interface IInMemoryConsumer<T> : IConsumer<T>
    {

    }
}
