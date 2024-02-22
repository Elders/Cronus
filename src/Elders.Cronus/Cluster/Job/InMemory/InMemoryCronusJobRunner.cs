using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Cluster.Job.InMemory;

public sealed class InMemoryCronusJobRunner : ICronusJobRunner
{
    static NoClusterOperations clusterOperations = new NoClusterOperations();

    public JobManager JobManager => throw new System.NotImplementedException();

    public void Dispose()
    {

    }

    public Task<JobExecutionStatus> ExecuteAsync(ICronusJob<object> job, CancellationToken cancellationToken = default)
    {
        return job.RunAsync(clusterOperations, cancellationToken);
    }
}
