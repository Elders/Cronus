using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Elders.Cronus.IntegrityValidation;

namespace Elders.Cronus.Migrations
{
    public class EventStoreValidator
    {
        private readonly IEventStorePlayer player;

        public EventStoreValidator(IEventStorePlayer player)
        {
            this.player = player;
        }

        IIntegrityPolicy<EventStream> DefaultPolicy()
        {
            var revValidator = new DuplicateRevisionsValidator();
            var missingRevValidator = new MissingRevisionsValidator();
            var orderValidator = new OrderedRevisionsValidator();

            var integrityPolicy = new EventStreamIntegrityPolicy();
            integrityPolicy.RegisterRule(new IntegrityRule<EventStream>(revValidator, new EmptyResolver()));
            integrityPolicy.RegisterRule(new IntegrityRule<EventStream>(missingRevValidator, new EmptyResolver()));
            integrityPolicy.RegisterRule(new IntegrityRule<EventStream>(orderValidator, new EmptyResolver()));

            return integrityPolicy;
        }

        public async Task ValidateAsync(EventStreamIntegrityPolicy policy)
        {
            var aggregateCommits = new List<AggregateCommit>();
            byte[] currentId = { 0 };
            await foreach (var commit in player.LoadAggregateCommitsAsync(1000))
            {
                if (ByteArrayHelper.Compare(currentId, commit.AggregateRootId) == false)
                {
                    if (aggregateCommits.Count > 0)
                    {
                        var result = policy.Apply(new EventStream(aggregateCommits));
                        if (result.IsIntegrityViolated)
                        {
                            throw new Exception(result.Output.ToString());
                        }
                        aggregateCommits.Clear();
                    }
                }

                aggregateCommits.Add(commit);
                currentId = commit.AggregateRootId;
            }
        }
    }
}
