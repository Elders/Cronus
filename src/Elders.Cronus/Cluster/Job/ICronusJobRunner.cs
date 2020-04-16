using System;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Cluster.Job
{
    public interface ICronusJobRunner : IDisposable
    {
        Task<JobExecutionStatus> ExecuteAsync(ICronusJob<object> job, CancellationToken cancellationToken = default);
    }
}
