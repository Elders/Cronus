namespace Elders.Cronus.EventStore
{
    public interface IEventStoreStorageManager
    {
        void CreateEventsStorage();

        /// <summary>
        /// Creates the database for the event store including the Events and Snapshots tables. If any exists it is not overwritten.
        /// </summary>
        void CreateStorage();

        void CreateSnapshotsStorage();
    }
}
