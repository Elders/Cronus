namespace Elders.Cronus.Pipeline
{
    public interface IConsumer<T>
    {

    }

    interface IInMemoryConsumer<T> : IConsumer<T>
    {

    }

    public interface ICronusConsumer<T> : IConsumer<T>
    {
        int NumberOfWorkers { get; }

        string Name { get; }

        void Start(int? numberOfWorkers = null);

        void Stop();
    }
}
