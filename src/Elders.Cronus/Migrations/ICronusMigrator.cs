using Elders.Cronus.EventStore;
using System.Threading.Tasks;

namespace Elders.Cronus.Migrations
{
    public interface ICronusMigrator
    {
        Task MigrateAsync(AggregateCommit aggregateCommit);
    }
}
