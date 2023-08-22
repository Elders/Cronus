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
                AggregateCommit commit1 = new AggregateCommit(aggregateId, 1, new List<IEvent>(), new List<IPublicEvent>(), DateTimeOffset.Now.ToFileTime());
                AggregateCommit commit3 = new AggregateCommit(aggregateId, 3, new List<IEvent>(), new List<IPublicEvent>(), DateTimeOffset.Now.ToFileTime());
                AggregateCommit commit4 = new AggregateCommit(aggregateId, 4, new List<IEvent>(), new List<IPublicEvent>(), DateTimeOffset.Now.ToFileTime());
                AggregateCommit commit10 = new AggregateCommit(aggregateId, 10, new List<IEvent>(), new List<IPublicEvent>(), DateTimeOffset.Now.ToFileTime());
                eventStream = new EventStream(new[] { commit1, commit3, commit4, commit10 });
                validator = new MissingRevisionsValidator();
            };

        Because of = () => validationResult = validator.Validate(eventStream);

        It should_report_about_the_invalid__EventStream__ = () => validationResult.IsValid.ShouldBeFalse();

        It should_report_all_missing_revisions = () => validationResult.Errors.Count().ShouldEqual(6);

        It should_have__OrderedRevisionsValidator_as_error_type = () => validationResult.ErrorType.ShouldEqual(nameof(OrderedRevisionsValidator));

        static EventStream eventStream;
        static MissingRevisionsValidator validator;
        static IValidatorResult validationResult;
    }
}
