using Machine.Specifications;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.AtomicAction.InMemory;
using System;

namespace Elders.Cronus.Tests.InMemoryEventStoreSuite
{
    [Subject("InMemoryLock")]
    public class When_locking_resource
    {
        Establish context = () =>
        {
            @lock = new InMemoryLock();
            resource = "locked";
        };

        Because of = () => locked = @lock.Lock(resource, TimeSpan.FromMinutes(1));

        It should_successfully_lock_the_resource = () => locked.ShouldBeTrue();

        static ILock @lock;
        static string resource;
        static bool locked;
    }
}
