//using Machine.Specifications;
//using Elders.Cronus.AtomicAction;
//using Elders.Cronus.AtomicAction.InMemory;
//using System;

//namespace Elders.Cronus.Tests.InMemoryEventStoreSuite
//{
//    [Subject("InMemoryLock")]
//    public class When_unlocking_a_specific_locked_resource
//    {
//        Establish context = () =>
//        {
//            @lock = new InMemoryLock();
//            resource1 = "locked1";
//            resource2 = "locked2";
//            @lock.Lock(resource1, TimeSpan.FromMinutes(1));
//            @lock.Lock(resource2, TimeSpan.FromMinutes(1));
//        };

//        Because of = () => @lock.Unlock(resource1);

//        It should_unlock_first_resource = () => @lock.IsLocked(resource1).ShouldBeFalse();

//        It should_not_unlock_second_resource = () => @lock.IsLocked(resource2).ShouldBeTrue();

//        static ILock @lock;
//        static string resource1;
//        static string resource2;
//    }
//}
