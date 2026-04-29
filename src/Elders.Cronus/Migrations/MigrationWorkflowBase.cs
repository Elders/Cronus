using System;
using System.Threading;
using System.Threading.Tasks;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Migrations;

/// <summary>
/// Base workflow that applies a single <see cref="IMigration{TInput, TResult}"/> over the supplied input.
/// </summary>
public abstract class MigrationWorkflowBase<TInput, TResult> : Workflow<TInput, TResult>
    where TInput : class
    where TResult : class
{
    static readonly ILogger logger = CronusLogger.CreateLogger(typeof(MigrationWorkflowBase<TInput, TResult>));

    protected readonly IMigration<TInput, TResult> migration;

    public MigrationWorkflowBase(IMigration<TInput, TResult> migration)
    {
        ArgumentNullException.ThrowIfNull(migration);

        this.migration = migration;
    }

    /// <inheritdoc />
    protected override Task<TResult> RunAsync(Execution<TInput, TResult> execution, CancellationToken cancellationToken = default)
    {
        TResult result = default(TResult);
        var input = execution.Context;
        try
        {
            if (migration.ShouldApply(input))
                result = migration.Apply(input);
        }
        catch (Exception ex) when (True(() => logger.LogError(ex, "Error while applying migration"))) { }

        return Task.FromResult(result);
    }
}
