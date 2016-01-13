using System;

namespace Elders.Cronus.MessageProcessing
{
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
            if (ReferenceEquals(null, disposeMe) == false)
                disposeMe.Dispose();
        }
    }
}