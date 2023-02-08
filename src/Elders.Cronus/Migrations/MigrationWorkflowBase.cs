using System;
using System.Threading.Tasks;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Migrations
{
    public abstract class MigrationWorkflowBase<TInput, TResult> : Workflow<TInput, TResult>
        where TInput : class
        where TResult : class
    {
        static readonly ILogger logger = CronusLogger.CreateLogger(typeof(MigrationWorkflowBase<TInput, TResult>));

        protected readonly IMigration<TInput, TResult> migration;

        public MigrationWorkflowBase(IMigration<TInput, TResult> migration)
        {
            if (migration is null) throw new ArgumentNullException(nameof(migration));

            this.migration = migration;
        }

        protected override Task<TResult> RunAsync(Execution<TInput, TResult> execution)
        {
            TResult result = default(TResult);
            var input = execution.Context;
            try
            {
                if (migration.ShouldApply(input))
                    result = migration.Apply(input);
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex, () => "Error while applying migration");
            }

            return Task.FromResult(result);
        }
    }
}
