//using Machine.Specifications;
//using Elders.Cronus.AtomicAction;
//using Elders.Cronus.AtomicAction.InMemory;

//namespace Elders.Cronus.Tests.InMemoryEventStoreSuite
//{
//    [Subject("InMemoryLock")]
//    public class When_checking_a_not_locked_resource
//    {
//        Establish context = () =>
//        {
//            @lock = new InMemoryLock();
//        };

//        Because of = () => locked = @lock.IsLocked("gg");

//        It should_be_unlocked = () => locked.ShouldBeFalse();

//        static ILock @lock;
//        static bool locked;
//    }
//}
