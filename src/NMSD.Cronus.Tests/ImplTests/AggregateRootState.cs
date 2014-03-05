using System;
using Machine.Specifications;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts;

namespace NMSD.Cronus.Tests.ImplTests
{
    [Subject("AggregateRootState_compare_operations")]
    public class When_compare_two_equal_AR_states
    {
        Establish context = () =>
        {
            var idBase = Guid.NewGuid();

            stateA = new AccountState();
            stateA.Id = new AccountId(idBase);
            stateA.Version = 1;

            stateB = new AccountState();
            stateB.Id = new AccountId(idBase);
            stateB.Version = 1;
        };

        Because of = () => result = stateA == stateB;

        It should_return__true__ = () => result.ShouldBeTrue();

        static bool result;
        static AccountState stateA;
        static AccountState stateB;
    }

    [Subject("AggregateRootState_compare_operations")]
    public class When_compare_two_AR_states_and_one_of_them_is__null__
    {
        Establish context = () =>
        {
            var idBase = Guid.NewGuid();

            stateA = new AccountState();
            stateA.Id = new AccountId(idBase);
            stateA.Version = 1;

            stateB = null;
        };

        Because of = () => result = stateA == stateB && stateB == stateA;

        It should_return__false__ = () => result.ShouldBeFalse();

        static bool result;
        static AccountState stateA;
        static AccountState stateB;
    }

    [Subject("AggregateRootState_compare_operations")]
    public class When_compare_two_AR_states_and_one_of_them_has_higher__Version__
    {
        Establish context = () =>
        {
            var idBase = Guid.NewGuid();

            stateA = new AccountState();
            stateA.Id = new AccountId(idBase);
            stateA.Version = 1;

            stateB = new AccountState();
            stateB.Id = new AccountId(idBase);
            stateB.Version = 2;
        };

        Because of = () => result = stateA < stateB && stateB > stateA;

        It should_return__true__ = () => result.ShouldBeTrue();

        static bool result;
        static AccountState stateA;
        static AccountState stateB;
    }
}
