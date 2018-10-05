//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using Elders.Cronus.Pipeline.Config;

//namespace Elders.Cronus.Discoveries
//{
//    public class ApplicationServiceDiscovery : DiscoveryBasedOnExecutingDirAssemblies
//    {
//        protected override void DiscoverFromAssemblies(ISettingsBuilder builder, IEnumerable<Assembly> assemblies)
//        {
//            string CRONUS = "AppServices";

//            builder
//                .UseCommandConsumer(CRONUS, consumer => consumer
//                    .UseApplicationServices(cmdHandler => cmdHandler
//                        .RegisterHandlersInAssembly(new[] { typeof(ProjectionVersionManagerAppService).Assembly }, IAA_appServiceFactory.Create).Middleware(x => new InMemoryRetryMiddlewareCustom<HandleContext>(x))));
//        }

//        //IEnumerable<Type> contracts = assemblies
//        //    .Where(asm => ReferenceEquals(default(BoundedContextAttribute), asm.GetAssemblyAttribute<BoundedContextAttribute>()) == false)
//        //    .SelectMany(ass => ass.GetExportedTypes());

//        //var serializer = new JsonSerializer(contracts);
//        //builder.Container.RegisterSingleton<ISerializer>(() => serializer);
//    }

//    public static T RegisterHandlersFromAssembly<T>(this T self, IEnumerable<Assembly> assemblies, Func<Type, object> messageHandlerFactory) where T : ISubscrptionMiddlewareSettings
//    {
//        var handlerTypes = messageHandlers.SelectMany(x => x.GetExportedTypes());
//        self.HandlerRegistrations = handlerTypes;
//        self.HandlerFactory = messageHandlerFactory;
//        return self;
//    }
//}

