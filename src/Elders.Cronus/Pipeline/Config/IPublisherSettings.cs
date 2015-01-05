using System;
using System.Reflection;
using Elders.Cronus.Serializer;
using Elders.Cronus.Serializer.Protoreg;
using Elders.Cronus.IocContainer;
using Elders.Proteus;

namespace Elders.Cronus.Pipeline.Config
{
    public static class PublisherSettingsExtensions
    {
        public static T UseContractsFromAssemblies<T>(this T self, Assembly[] assembliesContainingContracts = null, Type[] contracts = null)
            where T : ICanConfigureSerializer
        {
            var builder = self as ISettingsBuilder;
            var serializer = new ProteusSerializer(assembliesContainingContracts);
            builder.Container.RegisterSingleton<ISerializer>(() => serializer);
            return self;
        }

        //public static T UseContractsFromAssemblies<T>(this T self, Assembly[] assembliesContainingContracts, Type[] contracts = null)
        //    where T : ICanConfigureSerializer
        //{
        //    var builder = self as ISettingsBuilder;
        //    if (builder.Container.IsRegistered<ISerializer>())
        //        throw new InvalidOperationException("Protoreg serializer is already initialized.");

        //    var protoreg = new Elders.Protoreg.ProtoRegistration();
        //    protoreg.RegisterAssembly(typeof(CronusAssemby));
        //    protoreg.RegisterAssembly(typeof(IMessage));

        //    foreach (var ass in assembliesContainingContracts)
        //    {
        //        protoreg.RegisterAssembly(ass);
        //    }

        //    if (contracts != null)
        //        contracts.ToList().ForEach(c => protoreg.RegisterCommonType(c));

        //    var ser = new ProteusSerializer(protoreg);
        //    ser.Build();
        //    builder.Container.RegisterSingleton<ISerializer>(() => ser);
        //    return self;
        //}
    }
}