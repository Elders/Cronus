using System;
using System.Threading;
using Elders.Cronus.MessageProcessing;
using Machine.Specifications;

namespace Elders.Cronus.Tests
{
    public class When__SingletonPerTenant__is_used
    {
        Establish context = () =>
        {
            singleton = new SingletonPerTenant<DisposableObject>(new DummyProvider());
        };

        Because of = () => instance = singleton.Get();

        It should_run_fine = () => instance.Running.ShouldBeTrue();

        Cleanup cln = () =>
        {
            instance.Dispose();
        };

        static SingletonPerTenant<DisposableObject> singleton;
        static DisposableObject instance;
    }

    public class When__SingletonPerTenant__is_disposed
    {
        Establish context = () =>
        {
            singleton = new SingletonPerTenant<DisposableObject>(new DummyProvider());
            instance = singleton.Get();
        };

        Because of = () => singleton.Dispose();

        It should_dispose_properly = () => instance.Running.ShouldBeFalse();

        static SingletonPerTenant<DisposableObject> singleton;
        static DisposableObject instance;
    }

    public class DummyProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(CronusContext))
                return new CronusContext() { Tenant = "elders" };

            return new DisposableObject();
        }
    }

    public class DisposableObject : IDisposable
    {
        Thread trd;

        bool shouldRun = true;
        public bool Running { get; private set; }
        public DisposableObject()
        {
            Running = true;
            trd = new Thread(() =>
            {
                while (shouldRun)
                {
                    Thread.Sleep(1000);
                }

            });
        }
        public void Dispose()
        {
            Running = false;
        }
    }
}
