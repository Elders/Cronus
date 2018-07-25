using Machine.Specifications;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.AtomicAction.InMemory;
using System;

namespace Elders.Cronus.Tests.InMemoryEventStoreSuite
{
    [Subject("InMemoryLock")]
    public class When_checking_for_expired_locked_resource
    {
        Establish context = () =>
        {
            @lock = new InMemoryLock();
            resource = "locked";
            @lock.Lock(resource, TimeSpan.FromMinutes(-1));
        };

        Because of = () => locked = @lock.IsLocked(resource);

        It should_be_unlocked = () => locked.ShouldBeFalse();

        static ILock @lock;
        static string resource;
        static bool locked;
    }
}
