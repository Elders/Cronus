namespace NMSD.Cronus.EventSourcing.Config
{
    public interface IEventStoreBuilder
    {
        IEventStore Build();
    }
}