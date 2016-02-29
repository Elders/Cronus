using System;
using System.Collections.Generic;
using Elders.Cronus.EventStore;
using Elders.Cronus.IntegrityValidation;
using Machine.Specifications;

namespace Elders.Cronus.Tests.ValidatorsAndResolvers
{
    [Subject("IntegrityValidation")]
    public class When__AggregateCommit__has_duplicate_revisions
    {
        Establish context = () =>
            {
                byte[] aggregateId = Guid.NewGuid().ToByteArray();
                AggregateCommit commit1 = new AggregateCommit(aggregateId, "UnitTests", 1, new List<Cronus.DomainModeling.IEvent>());
                eventStream = new EventStream(new[] { commit1, commit1, commit1 });
                duplicateRevisionsvalidator = new DuplicateRevisionsValidator();
            };

        Because of = () => validationResult = duplicateRevisionsvalidator.Validate(eventStream);

        It should_report_about_the_invalid__EventStream__ = () => validationResult.IsValid.ShouldBeFalse();

        It should_have__DuplicateRevisionValidator_as_error_type = () => validationResult.ErrorType.ShouldEqual(nameof(DuplicateRevisionsValidator));

        static EventStream eventStream;
        static DuplicateRevisionsValidator duplicateRevisionsvalidator;
        static IValidatorResult validationResult;
    }

    [Subject("IntegrityValidation")]
    public class When__EventStreamPolicy_is_applied_to_a_correct__EventStream__
    {
        Establish context = () =>
            {
                byte[] aggregateId = Guid.NewGuid().ToByteArray();
                AggregateCommit commit1 = new AggregateCommit(aggregateId, "UnitTests", 1, new List<Cronus.DomainModeling.IEvent>());
                AggregateCommit commit2 = new AggregateCommit(aggregateId, "UnitTests", 2, new List<Cronus.DomainModeling.IEvent>());
                AggregateCommit commit3 = new AggregateCommit(aggregateId, "UnitTests", 3, new List<Cronus.DomainModeling.IEvent>());
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