using System;
using Machine.Specifications;
using Elders.Cronus.Tests.TestModel;

namespace Elders.Cronus.Tests.ImplTests
{
    [Subject("AggregateRootState_compare_operations")]
    public class When_compare_two_equal_AR_states
    {
        Establish context = () =>
        {
            var idBase = Guid.NewGuid();

            stateA = new TestAggregateRootState();
            stateA.Id = new TestAggregateId(idBase);
            stateA.Version = 1;

            stateB = new TestAggregateRootState();
            stateB.Id = new TestAggregateId(idBase);
            stateB.Version = 1;
        };

        Because of = () => result = stateA == stateB;

        It should_return__true__ = () => result.ShouldBeTrue();

        static bool result;
        static TestAggregateRootState stateA;
        static TestAggregateRootState stateB;
    }

    [Subject("AggregateRootState_compare_operations")]
    public class When_compare_two_AR_states_and_one_of_them_is__null__
    {
        Establish context = () =>
        {
            var idBase = Guid.NewGuid();

            stateA = new TestAggregateRootState();
            stateA.Id = new TestAggregateId(idBase);
            stateA.Version = 1;

            stateB = null;
        };

        Because of = () => result = stateA == stateB && stateB == stateA;

        It should_return__false__ = () => result.ShouldBeFalse();

        static bool result;
        static TestAggregateRootState stateA;
        static TestAggregateRootState stateB;
    }

    [Subject("AggregateRootState_compare_operations")]
    public class When_compare_two_AR_states_and_one_of_them_has_higher__Version__
    {
        Establish context = () =>
        {
            var idBase = Guid.NewGuid();

            stateA = new TestAggregateRootState();
            stateA.Id = new TestAggregateId(idBase);
            stateA.Version = 1;

            stateB = new TestAggregateRootState();
            stateB.Id = new TestAggregateId(idBase);
            stateB.Version = 2;
        };

        Because of = () => result = stateA < stateB && stateB > stateA;

        It should_return__true__ = () => result.ShouldBeTrue();

        static bool result;
        static TestAggregateRootState stateA;
        static TestAggregateRootState stateB;
    }
}
