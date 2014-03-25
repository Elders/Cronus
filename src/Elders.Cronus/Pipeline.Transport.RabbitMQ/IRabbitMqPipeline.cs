namespace Elders.Cronus.Pipeline.Transport.RabbitMQ
{
    public interface IRabbitMqPipeline : IPipeline
    {
        void Open();

        void Close();

        void Declare();
    }
}