using Elders.Cronus.EventStore.Index;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Cluster.Job
{
    public abstract class CronusJob<TData> : ICronusJob<TData>
        where TData : class, IJobData, new()
    {
        Func<TData, TData> DataOverride = fromCluster => fromCluster;

        protected readonly ILogger<CronusJob<TData>> logger;

        public CronusJob(ILogger<CronusJob<TData>> logger)
        {
            this.logger = logger;

            Data = BuildInitialData();
        }

        public abstract string Name { get; set; }

        /// <summary>
        /// Initializes a default state for the job
        /// </summary>
        /// <returns>Returns the state data</returns>
        protected abstract TData BuildInitialData();

        public Task<JobExecutionStatus> RunAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            try
            {
                using (logger.BeginScope(s => s
                    .AddScope("cronus_job_name", Name)))
                {
                    return Task.Factory
                        .ContinueWhenAll(new Task[] { SyncInitialStateAsync(cluster, cancellationToken) }, _ => Task.CompletedTask)
                        .ContinueWith(x => RunJobWithLoggerAsync(cluster, cancellationToken))
                        .Result;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => "Job run has failed.");

                return Task.FromResult(JobExecutionStatus.Failed);
            }
        }

        public async Task SyncInitialStateAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            Data = await cluster.PingAsync<TData>(cancellationToken).ConfigureAwait(false);

            if (Data is null)
                Data = BuildInitialData();

            Data = DataOverride(Data);
        }

        private Task<JobExecutionStatus> RunJobWithLoggerAsync(IClusterOperations cluster, CancellationToken cancellationToken = default)
        {
            using (logger.BeginScope(s => s
                       .AddScope("cronus_job_data", System.Text.Json.JsonSerializer.Serialize<TData>(Data))))
            {
                logger.Info(() => "Job started...");
                return RunJobAsync(cluster, cancellationToken);
            }
        }

        protected abstract Task<JobExecutionStatus> RunJobAsync(IClusterOperations cluster, CancellationToken cancellationToken = default);

        public TData Data { get; protected set; }

        public void OverrideData(Func<TData, TData> dataOverride)
        {
            DataOverride = dataOverride;
        }

        protected virtual TData Override(TData fromCluster, TData fromLocal)
        {
            if (fromCluster.IsCompleted && fromCluster.Timestamp < fromLocal.Timestamp)
                return fromLocal;
            else
                return fromCluster;
        }

        public virtual Task BeforeRunAsync() { return Task.CompletedTask; }

        public virtual Task AfterRunAsync() { return Task.CompletedTask; }
    }
}
