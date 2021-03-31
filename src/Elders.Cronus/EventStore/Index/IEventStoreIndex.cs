namespace Elders.Cronus.EventStore.Index
{
    public interface IEventStoreIndex : ISystemHandler, IMessageHandler
    {
        void Index(CronusMessage message);
    }
}
