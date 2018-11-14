using System;
using Elders.Cronus.Logging;
using Elders.Cronus.Workflow;

namespace Elders.Cronus.Migrations
{
    public class GenericMigrationWorkflow<T, V> : Workflow<T, V>
    {
        static readonly ILog log = LogProvider.GetLogger(typeof(GenericMigrationWorkflow<T, V>));

        protected readonly IMigration<T, V> migration;

        public GenericMigrationWorkflow(IMigration<T, V> migration)
        {
            if (migration is null) throw new ArgumentNullException(nameof(migration));

            this.migration = migration;
        }

        protected override V Run(Execution<T, V> context)
        {
            V result = default(V);
            var commit = context.Context;
            try
            {
                if (migration.ShouldApply((T)commit))
                    result = migration.Apply((T)commit);
            }
            catch (Exception ex)
            {
                log.ErrorException("Error while applying migration", ex);
            }

            return result;
        }
    }
}
