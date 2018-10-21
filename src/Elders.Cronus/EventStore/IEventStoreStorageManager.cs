namespace Elders.Cronus.EventStore
{
    public interface IEventStoreStorageManager
    {


        /// <summary>
        /// Creates the database for the event store including the Events and Snapshots tables. If any exists it is not overwritten.
        /// </summary>
        void CreateStorage();

        void CreateEventsStorage();
        void CreateIndecies();
        void CreateSnapshotsStorage();
    }
}
