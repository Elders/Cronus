using System;
using System.Collections.Generic;
using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Integrity;
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
                AggregateCommit commit1 = new AggregateCommit(aggregateId, 1, new List<Cronus.IEvent>(), new List<IPublicEvent>(), DateTimeOffset.Now.ToFileTime());
                AggregateCommit commit2 = new AggregateCommit(aggregateId, 2, new List<Cronus.IEvent>(), new List<IPublicEvent>(), DateTimeOffset.Now.ToFileTime());
                AggregateCommit commit3 = new AggregateCommit(aggregateId, 3, new List<Cronus.IEvent>(), new List<IPublicEvent>(), DateTimeOffset.Now.ToFileTime());
                eventStream = new EventStream(new[] { commit1, commit3, commit2 });

                integrityPolicy = new EventStreamIntegrityPolicy();
            };

        Because of = () => integrityResult = integrityPolicy.Apply(eventStream);

        It should_report_ = () => integrityResult.IsIntegrityViolated.ShouldBeFalse();

        static EventStream eventStream;
        static EventStreamIntegrityPolicy integrityPolicy;
        static IntegrityResult<EventStream> integrityResult;
    }
}
