using System;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Cluster.Job
{
    /// <summary>
    /// Allows executing a <see cref="ICronusJob{TData}"/> in a cluster environment.
    /// </summary>
    /// <remarks>Consider implementing <see cref="IClusterOperations"/> as well.</remarks>
    public interface ICronusJobRunner : IDisposable
    {
        /// <summary>
        /// Executes the <see cref="ICronusJob{TData}"/>
        /// </summary>
        /// <param name="job">The job.</param>
        /// <returns>Returns the <see cref="JobExecutionStatus"/> of the job after execution.</returns>
        Task<JobExecutionStatus> ExecuteAsync(ICronusJob<object> job, CancellationToken cancellationToken = default);
    }
}
