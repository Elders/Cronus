namespace Elders.Cronus.EventStore.Index
{
    public interface ICronusEventStoreIndex : IEventStoreIndex, ISystemHandler, IMessageHandler
    {
    }

    public interface IEventStoreIndex : IMessageHandler
    {
        void Index(CronusMessage message);
    }
}
