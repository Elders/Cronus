namespace Elders.Cronus.EventStore.InMemory
{
    public class InMemoryEventStoreStorageManager : IEventStoreStorageManager
    {
        public void CreateStorage()
        {
            this.CreateEventsStorage();
            this.CreateSnapshotsStorage();
        }

        public void CreateEventsStorage()
        {

        }

        public void CreateSnapshotsStorage()
        {

        }
    }
}