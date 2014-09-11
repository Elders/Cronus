namespace Elders.Cronus.Pipeline
{
    public interface IEndpointConsumerHost
    {
        int NumberOfWorkers { get; }

        void Start(int? numberOfWorkers = null);

        void Stop();
    }
}