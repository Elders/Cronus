using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Cluster.Job
{
    public abstract class CronusJob<TData> : ICronusJob<TData>
        where TData : class, new()
    {
        public CronusJob()
        {
            Data = BuildInitialData();
        }


        public abstract string Name { get; }

        /// <summary>
        /// Initializes a default state for the job
        /// </summary>
        /// <returns>Returns the state data</returns>
        public abstract TData BuildInitialData();

        public Task<JobExecutionStatus> RunAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            return Task.Factory
                .ContinueWhenAll(new Task[] { SyncInitialStateAsync(cluster) }, _ => Task.CompletedTask)
                .ContinueWith(x => RunJob(cluster, cancellationToken))
                .Result;
        }

        public async Task SyncInitialStateAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            Data = await cluster.PingAsync<TData>(cancellationToken);

            if (Data is null)
                Data = BuildInitialData();
        }

        protected abstract Task<JobExecutionStatus> RunJob(IClusterOperations cluster, CancellationToken cancellationToken = default);

        public TData Data { get; protected set; }
    }
}
