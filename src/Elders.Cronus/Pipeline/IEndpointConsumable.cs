namespace Elders.Cronus.Pipeline
{
    public interface IEndpointConsumable
    {
        int NumberOfWorkers { get; }

        void Start(int? numberOfWorkers = null);

        void Stop();
    }
}