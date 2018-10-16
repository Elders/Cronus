namespace Elders.Cronus.Projections
{
    public interface IProjectionStoreStorageManager
    {
        void CreateProjectionsStorage();

        /// <summary>
        /// Creates the database for the projection store including the Projections and Snapshots tables. If any exists it is not overwritten.
        /// </summary>
        void CreateStorage();
    }
}
