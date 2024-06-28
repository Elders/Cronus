using System;
using System.Threading.Tasks;
using Elders.Cronus.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.MessageProcessing;

public interface IHandlerFactory
{
    IHandlerInstance Create(Type handlerType);
}

public class DefaultHandlerFactory : IHandlerFactory
{
    private readonly ICronusContextAccessor cronusContextAccessor;

    public DefaultHandlerFactory(ICronusContextAccessor cronusContextAccessor)
    {
        this.cronusContextAccessor = cronusContextAccessor;
    }

    public IHandlerInstance Create(Type handlerType)
    {
        object handlerInstance = cronusContextAccessor.CronusContext.ServiceProvider.GetRequiredService(handlerType);

        return new DefaultHandlerInstance(handlerInstance);
    }
}

public interface IHandlerInstance : IDisposable
{
    object Current { get; }
}

public class DefaultHandlerInstance : IHandlerInstance
{
    public DefaultHandlerInstance(object instance)
    {
        Current = instance;
    }

    public object Current { get; set; }

    public void Dispose()
    {
        var disposeMe = Current as IDisposable;
        if (disposeMe is null == false)
            disposeMe.Dispose();
    }
}
