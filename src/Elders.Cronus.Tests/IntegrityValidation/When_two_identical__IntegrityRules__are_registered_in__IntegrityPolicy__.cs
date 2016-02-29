using System.Linq;
using Elders.Cronus.EventStore;
using Elders.Cronus.IntegrityValidation;
using Machine.Specifications;

namespace Elders.Cronus.Tests.ValidatorsAndResolvers
{
    [Subject("IntegrityValidation")]
    public class When_two_identical__IntegrityRules__are_registered_in__IntegrityPolicy__
    {
        Establish context = () =>
            {
                rule1 = new IntegrityRule<EventStream>(new DuplicateRevisionsValidator(), new EmptyResolver());
                rule2 = new IntegrityRule<EventStream>(new DuplicateRevisionsValidator(), new EmptyResolver());
                policy = new EventStreamIntegrityPolicy();
                policy.RegisterRule(rule1);
            };

        Because of = () => policy.RegisterRule(rule2);

        It should_register_only_one_rule_with_that_resolver =
            () => policy.Rules.Where(x => x.Resolver.GetType() == typeof(EmptyResolver)).Count().ShouldEqual(1);

        It should_have_two_rules_because_logger_is_registered_implicitly =
            () => policy.Rules.Count().ShouldEqual(2);

        static IntegrityRule<EventStream> rule1;
        static IntegrityRule<EventStream> rule2;
        static EventStreamIntegrityPolicy policy;
    }
}