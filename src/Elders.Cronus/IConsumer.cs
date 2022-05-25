namespace Elders.Cronus
{
    public interface IConsumer<out T> where T : IMessageHandler
    {
        void Start();

        void Stop();
    }
}
