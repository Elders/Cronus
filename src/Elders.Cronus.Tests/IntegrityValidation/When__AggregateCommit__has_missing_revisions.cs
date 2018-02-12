using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.EventStore;
using Elders.Cronus.IntegrityValidation;
using Machine.Specifications;

namespace Elders.Cronus.Tests.ValidatorsAndResolvers
{
    [Subject("IntegrityValidation")]
    public class When__AggregateCommit__has_missing_revisions
    {
        Establish context = () =>
            {
                byte[] aggregateId = Guid.NewGuid().ToByteArray();
                AggregateCommit commit1 = new AggregateCommit(aggregateId, "UnitTests", 1, new List<IEvent>());
                AggregateCommit commit3 = new AggregateCommit(aggregateId, "UnitTests", 3, new List<IEvent>());
                AggregateCommit commit4 = new AggregateCommit(aggregateId, "UnitTests", 4, new List<IEvent>());
                AggregateCommit commit6 = new AggregateCommit(aggregateId, "UnitTests", 10, new List<IEvent>());
                eventStream = new EventStream(new[] { commit1, commit3, commit4, commit6 });
                validator = new MissingRevisionsValidator();
            };

        Because of = () => validationResult = validator.Validate(eventStream);

        It should_report_about_the_invalid__EventStream__ = () => validationResult.IsValid.ShouldBeFalse();

        It should_report_all_missing_revisions = () => validationResult.Errors.Count().ShouldEqual(1);

        It should_have__OrderedRevisionsValidator_as_error_type = () => validationResult.ErrorType.ShouldEqual(nameof(OrderedRevisionsValidator));

        static EventStream eventStream;
        static MissingRevisionsValidator validator;
        static IValidatorResult validationResult;
    }
}
