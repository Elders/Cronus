using Elders.Cronus.EventStore;
using Elders.Cronus.EventStore.Integrity;
using Elders.Cronus.IntegrityValidation;
using Machine.Specifications;

namespace Elders.Cronus.Tests.ValidatorsAndResolvers;

[Subject("IntegrityValidation")]
public class When_comparing_two__IntegrityRules__are_identical
{
    Establish context = () =>
        {
            rule1 = new IntegrityRule<EventStream>(new DuplicateRevisionsValidator(), new EmptyResolver());
            rule2 = new IntegrityRule<EventStream>(new DuplicateRevisionsValidator(), new EmptyResolver());
        };

    Because of = () => areIdentical = rule1 == rule2 && rule1.Equals(rule2);

    It should_have_positive_result = () => areIdentical.ShouldBeTrue();

    static bool areIdentical;
    static IntegrityRule<EventStream> rule1;
    static IntegrityRule<EventStream> rule2;
}
