using System;
using System.Collections.Generic;
using Elders.Cronus.EventStore;
using Elders.Cronus.IntegrityValidation;
using Machine.Specifications;

namespace Elders.Cronus.Tests.ValidatorsAndResolvers
{
    [Subject("IntegrityValidation")]
    public class When__AggregateCommit__has_unordered_revisions_and_resolving_them
    {
        Establish context = () =>
            {
                byte[] aggregateId = Guid.NewGuid().ToByteArray();
                AggregateCommit commit1 = new AggregateCommit(aggregateId, "UnitTests", 1, new List<Cronus.IEvent>());
                AggregateCommit commit2 = new AggregateCommit(aggregateId, "UnitTests", 2, new List<Cronus.IEvent>());
                AggregateCommit commit4 = new AggregateCommit(aggregateId, "UnitTests", 4, new List<Cronus.IEvent>());
                eventStream = new EventStream(new[] { commit2, commit4, commit1 });
                validator = new OrderedRevisionsValidator();
                validationResult = validator.Validate(eventStream);
                resolver = new UnorderedRevisionsResolver();
            };

        Because of = () => integrityResult = resolver.Resolve(eventStream, validationResult);

        It should_report_about_valid__EventStream__ = () => integrityResult.IsIntegrityViolated.ShouldBeFalse();

        It should_report_about_valid_validation_result = () => validator.Validate(integrityResult.Output).IsValid.ShouldBeTrue();

        static EventStream eventStream;
        static OrderedRevisionsValidator validator;
        static IValidatorResult validationResult;
        static UnorderedRevisionsResolver resolver;
        static IntegrityResult<EventStream> integrityResult;
    }
}
