namespace Elders.Cronus.Persistence.MSSQL
{
    public interface IMsSqlEventStoreTableNameStrategy
    {
        string GetEventsTableName();

        string GetSnapshotsTableName();
    }
}