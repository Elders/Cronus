using System;
using System.Threading.Tasks;
using Elders.Cronus.Workflow;

namespace Elders.Cronus.MessageProcessing
{
    public interface IHandlerFactory
    {
        IHandlerInstance Create(Type handlerType);
    }

    public class DefaultHandlerFactory : IHandlerFactory
    {
        public static Workflow<HandleContext, IHandlerInstance> FactoryWrokflow = WorkflowExtensions.Lambda<HandleContext, IHandlerInstance>(exec =>
        { return Task.FromResult(factory.Create(exec.Context.HandlerType)); });

        readonly Func<Type, object> handlerFctory;

        public DefaultHandlerFactory(Func<Type, object> handlerFactory)
        {
            this.handlerFctory = handlerFactory;
        }

        public IHandlerInstance Create(Type handlerType)
        {
            return new DefaultHandlerInstance(handlerFctory(handlerType));
        }

        private static DefaultHandlerFactory factory = new DefaultHandlerFactory(x => FastActivator.CreateInstance(x));
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
}
