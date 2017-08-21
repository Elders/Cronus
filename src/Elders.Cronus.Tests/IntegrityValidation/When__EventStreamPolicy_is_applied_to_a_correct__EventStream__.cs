using System;
using System.Collections.Generic;
using Elders.Cronus.EventStore;
using Elders.Cronus.IntegrityValidation;
using Machine.Specifications;

namespace Elders.Cronus.Tests.ValidatorsAndResolvers
{
    [Subject("IntegrityValidation")]
    public class When__EventStreamPolicy_is_applied_to_a_correct__EventStream__
    {
        Establish context = () =>
            {
                byte[] aggregateId = Guid.NewGuid().ToByteArray();
                AggregateCommit commit1 = new AggregateCommit(aggregateId, "UnitTests", 1, new List<Cronus.IEvent>());
                AggregateCommit commit2 = new AggregateCommit(aggregateId, "UnitTests", 2, new List<Cronus.IEvent>());
                AggregateCommit commit3 = new AggregateCommit(aggregateId, "UnitTests", 3, new List<Cronus.IEvent>());
                eventStream = new EventStream(new[] { commit1, commit3, commit2 });
                duplicateRevisionsvalidator = new DuplicateRevisionsValidator();

                integrityPolicy = new EventStreamIntegrityPolicy();
                integrityPolicy.RegisterRule(new IntegrityRule<EventStream>(new MissingRevisionsValidator(), new EmptyResolver()));
                integrityPolicy.RegisterRule(new IntegrityRule<EventStream>(new OrderedRevisionsValidator(), new UnorderedRevisionsResolver()));
            };

        Because of = () => integrityResult = integrityPolicy.Apply(eventStream);

        It should_report_ = () => integrityResult.IsIntegrityViolated.ShouldBeFalse();

        static EventStream eventStream;
        static DuplicateRevisionsValidator duplicateRevisionsvalidator;
        static EventStreamIntegrityPolicy integrityPolicy;
        static IntegrityResult<EventStream> integrityResult;
    }
}
