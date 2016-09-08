using System;

namespace Elders.Cronus.Pipeline.Config
{
    public class InternalApplicationServiceFactory : IMessageHandlerFactory
    {
        readonly IMessageHandlerFactory externalFactory;

        public InternalApplicationServiceFactory(IMessageHandlerFactory externalFactory)
        {
            this.externalFactory = externalFactory;
        }

        public object CreateHandler(Type t)
        {
            object instance = externalFactory == null
                ? FastActivator.CreateInstance(t)
                : externalFactory.CreateHandler(t);

            return instance;
        }
    }
}