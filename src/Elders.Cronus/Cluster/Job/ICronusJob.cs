using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Cluster.Job
{
    public interface ICronusJob<out TData> where TData : class
    {
        /// <summary>
        /// The name of the job
        /// </summary>
        string Name { get; }

        public TData Data { get; }

        Task SyncInitialStateAsync(IClusterOperations cluster, CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs the job
        /// </summary>
        Task<JobExecutionStatus> RunAsync(IClusterOperations cluster, CancellationToken cancellationToken = default);
    }
}
