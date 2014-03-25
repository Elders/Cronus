namespace Elders.Cronus.Pipeline
{
    public interface IEndpointConsumable
    {
        void Start(int numberOfWorkers);

        void Stop();
    }
}