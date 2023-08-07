﻿using Elders.Cronus.Migration.Middleware.Tests.TestModel.Foo;
using Elders.Cronus.EventStore;
using System.Collections.Generic;
using System.Text;
using System;

namespace Elders.Cronus.Migrations.TestMigration
{
    public class RemoveEventMigration : IMigration<AggregateCommit, IEnumerable<AggregateCommit>>
    {
        readonly string targetAggregateName = "Foo".ToLowerInvariant();
        static readonly FooId id = new FooId("1234", "elders");
        static readonly string contractIdToRemove = typeof(TestUpdateEventFoo).GetContractId();

        public IEnumerable<AggregateCommit> Apply(AggregateCommit current)
        {
            if (ShouldApply(current))
            {
                var newEvents = new List<IEvent>(current.Events);
                newEvents.RemoveAll(x => x.GetType().GetContractId() == contractIdToRemove);
                var newAggregateCommit = new AggregateCommit(current.AggregateRootId, current.Revision, newEvents, new List<IPublicEvent>(), DateTimeOffset.Now.ToFileTime());

                yield return newAggregateCommit;
            }

            else
                yield return current;
        }

        public bool ShouldApply(AggregateCommit current)
        {
            var urnRaw = new Urn(Encoding.UTF8.GetString(current.AggregateRootId));
            var urn = AggregateRootId.Parse(urnRaw.Value);
            string currentAggregateName = urn.AggregateRootName;

            if (currentAggregateName == targetAggregateName)
                return true;

            return false;
        }
    }
}
