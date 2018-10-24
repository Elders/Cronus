using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus
{
    public class TypeContainer<T> : ITypeContainer
    {
        public TypeContainer(IEnumerable<Type> items)
        {
            var expectedType = typeof(T);
            Items = items.Where(type => expectedType.IsAssignableFrom(type)).ToList();
        }

        public List<Type> Items { get; set; }
    }


    //public class SystemProjectionMessageProcessorSettings : SettingsBuilder, ISubscrptionMiddlewareSettings
    //{
    //    public SystemProjectionMessageProcessorSettings(ISettingsBuilder builder) : base(builder)
    //    {
    //        (this as ISubscrptionMiddlewareSettings).HandleMiddleware = (x) => x;
    //    }

    //    IEnumerable<Type> ISubscrptionMiddlewareSettings.HandlerRegistrations { get; set; }

    //    Func<Type, object> ISubscrptionMiddlewareSettings.HandlerFactory { get; set; }

    //    Func<Middleware<HandleContext>, Middleware<HandleContext>> ISubscrptionMiddlewareSettings.HandleMiddleware { get; set; }

    //    public override void Build()
    //    {
    //        var hasher = new ProjectionHasher();
    //        var builder = this as ISettingsBuilder;
    //        var processorSettings = this as ISubscrptionMiddlewareSettings;
    //        Func<SubscriptionMiddleware> messageHandlerProcessorFactory = () =>
    //        {
    //            var handlerFactory = new DefaultHandlerFactory(processorSettings.HandlerFactory);

    //            var clusterServiceMiddleware = new SystemMiddleware(handlerFactory);
    //            var middleware = processorSettings.HandleMiddleware(clusterServiceMiddleware);
    //            var subscriptionMiddleware = new SubscriptionMiddleware();
    //            var clusterSettings = builder.Container.Resolve<IClusterSettings>();
    //            string nodeId = $"{clusterSettings.CurrentNodeName}@{clusterSettings.ClusterName}";
    //            foreach (var reg in (this as ISubscrptionMiddlewareSettings).HandlerRegistrations)
    //            {
    //                string subscriberId = reg.FullName + "." + nodeId;
    //                if (typeof(ISystemProjection).IsAssignableFrom(reg) && reg.IsClass)
    //                {
    //                    subscriptionMiddleware.Subscribe(new HandlerSubscriber(reg, middleware, subscriberId));

    //                    var transport = builder.Container.Resolve<ITransport>(builder.Name);
    //                    var serializer = builder.Container.Resolve<ISerializer>(null);
    //                    var publisher = transport.GetPublisher<ICommand>(serializer);
    //                    var projManagerId = new ProjectionVersionManagerId(reg.GetContractId());
    //                    var command = new RegisterProjection(projManagerId, hasher.CalculateHash(reg).ToString());
    //                    publisher.Publish(command);
    //                }
    //            }
    //            return subscriptionMiddleware;
    //        };
    //        builder.Container.RegisterSingleton<SubscriptionMiddleware>(() => messageHandlerProcessorFactory(), builder.Name);
    //    }
    //}
}
