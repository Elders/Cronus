using System;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Migrations
{
    public class MigrationWorkflowBase<TInput, TResult> : Workflow<TInput, TResult> where TInput : class
    {
        readonly ILogger logger = CronusLogger.CreateLogger(typeof(MigrationWorkflowBase<TInput, TResult>));

        protected readonly IMigration<TInput, TResult> migration;

        public MigrationWorkflowBase(IMigration<TInput, TResult> migration)
        {
            if (migration is null) throw new ArgumentNullException(nameof(migration));

            this.migration = migration;
        }

        protected override TResult Run(Execution<TInput, TResult> context)
        {
            TResult result = default(TResult);
            var input = context.Context;
            try
            {
                if (migration.ShouldApply(input))
                    result = migration.Apply(input);
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error while applying migration", ex);
            }

            return result;
        }
    }
}
