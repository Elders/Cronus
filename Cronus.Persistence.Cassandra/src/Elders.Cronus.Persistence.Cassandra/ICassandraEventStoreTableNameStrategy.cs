namespace Elders.Cronus.Persistence.Cassandra
{
    public interface ICassandraEventStoreTableNameStrategy
    {
        string GetEventsTableName();

        string GetSnapshotsTableName();
    }
}
