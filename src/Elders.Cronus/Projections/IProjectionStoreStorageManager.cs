using System.Threading.Tasks;

namespace Elders.Cronus.Projections;

public interface IProjectionStoreStorageManager
{
    Task CreateProjectionsStorageAsync(string location);
}
