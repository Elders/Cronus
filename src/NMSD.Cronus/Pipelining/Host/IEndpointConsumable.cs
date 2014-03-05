namespace NMSD.Cronus.Hosting
{
    public interface IEndpointConsumable
    {
        void Start(int numberOfWorkers);

        void Stop();
    }
}