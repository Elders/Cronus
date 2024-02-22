using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus.Cluster.Job;

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

    JobManager JobManager { get; }
}

public abstract class AbstractCronusJobRunner : ICronusJobRunner
{
    private readonly JobManager jobManager;

    public AbstractCronusJobRunner(JobManager jobManager)
    {
        this.jobManager = jobManager;
    }

    public JobManager JobManager => jobManager;

    public Task<JobExecutionStatus> ExecuteAsync(ICronusJob<object> job, CancellationToken cancellationToken = default)
    {
        CancellationTokenSource jobCancelationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        jobManager.Register(job.Name, jobCancelationTokenSource);

        return ExecuteInternalAsync(job, jobCancelationTokenSource.Token);
    }

    protected abstract Task<JobExecutionStatus> ExecuteInternalAsync(ICronusJob<object> job, CancellationToken cancellationToken = default);

    public abstract void Dispose();
}

public sealed class JobManager
{
    private ConcurrentDictionary<string, CancellationTokenSource> jobRegistry;

    public JobManager()
    {
        jobRegistry = new ConcurrentDictionary<string, CancellationTokenSource>();
    }

    public bool Register(string jobId, CancellationTokenSource cts)
    {
        return jobRegistry.TryAdd(jobId, cts);
    }

    public bool Register(string jobId, CancellationToken ct)
    {
        return Register(jobId, CancellationTokenSource.CreateLinkedTokenSource(ct));
    }

    public Task CancelAsync(string[] jobIds)
    {
        List<Task> tasks = new List<Task>();

        for (int i = 0; i < jobIds.Length; i++)
        {
            tasks.Add(CancelAsync(jobIds[i]));
        }

        return Task.WhenAll(tasks);
    }

    public Task CancelAsync(string jobId)
    {
        if (jobRegistry.TryRemove(jobId, out CancellationTokenSource cts))
            return cts.CancelAsync();

        return Task.CompletedTask;
    }

    public Task CancelAllAsync()
    {
        List<Task> tasks = new List<Task>();

        foreach (var jobRegistry in jobRegistry)
            tasks.Add(jobRegistry.Value.CancelAsync());

        return Task.WhenAll(tasks);
    }
}
