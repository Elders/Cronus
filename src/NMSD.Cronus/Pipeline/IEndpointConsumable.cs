namespace NMSD.Cronus.Pipelining
{
    public interface IEndpointConsumable
    {
        void Start(int numberOfWorkers);

        void Stop();
    }
}