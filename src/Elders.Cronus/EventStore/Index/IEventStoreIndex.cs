namespace Elders.Cronus.EventStore.Index
{
    public interface IEventStoreIndex : IMessageHandler
    {
        void Index(CronusMessage message);
    }
}
