namespace Elders.Cronus.Pipeline
{
    public interface IEndpointConsumer
    {
        int NumberOfWorkers { get; }

        void Start(int? numberOfWorkers = null);

        void Stop();
    }
}