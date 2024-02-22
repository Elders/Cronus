using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;

namespace Elders.Cronus.Tests;

public class When__SingletonPerTenantContainer__is_used
{
    Establish context = () =>
    {
        container = new SingletonPerTenantContainer<DisposableObject>();
        instance = new DisposableObject();
        container.Stash.TryAdd("tenant", instance);
    };

    Because of = () => container.Stash.TryGetValue("tenant", out instance);

    It should_run_fine = () => instance.Running.ShouldBeTrue();

    Cleanup cleanup = () =>
    {
        instance.Dispose();
    };

    static SingletonPerTenantContainer<DisposableObject> container;
    static DisposableObject instance;
}

public class When__SingletonPerTenantContainer__is_disposed
{
    Establish context = () =>
    {
        container = new SingletonPerTenantContainer<DisposableObject>();
        instance = new DisposableObject();
        container.Stash.TryAdd("tenant", instance);
    };

    Because of = () => container.Dispose();

    It should_dispose_the_instance = () => instance.Running.ShouldBeFalse();

    It should_clear_the_container = () => container.Stash.Any().ShouldBeFalse();

    static SingletonPerTenantContainer<DisposableObject> container;
    static DisposableObject instance;
}

public class DisposableObject : IDisposable
{
    public bool Running { get; private set; }

    public DisposableObject()
    {
        Running = true;
        Task.Factory.StartNew(() =>
        {
            while (Running)
            {
                Thread.Sleep(100);
            }

        });
    }
    public void Dispose()
    {
        Running = false;
    }
}
