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
                AggregateCommit commit1 = new AggregateCommit(aggregateId, "UnitTests", 1, new List<IEvent>());
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
}
