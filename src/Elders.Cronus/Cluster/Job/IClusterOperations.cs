using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Cluster.Job
{
    public interface IClusterOperations
    {
        Task<TData> PingAsync<TData>(CancellationToken cancellationToken = default) where TData : class, new();
        Task<TData> PingAsync<TData>(TData data, CancellationToken cancellationToken = default) where TData : class, new();
    }
}
