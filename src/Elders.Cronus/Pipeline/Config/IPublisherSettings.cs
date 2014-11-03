using System;
using System.Linq;
using System.Reflection;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Serializer;
using Elders.Cronus.Serializer.Protoreg;
using Elders.Cronus.IocContainer;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IPipelinePublisherSettings<TContract> : ISettingsBuilder where TContract : IMessage
    {

    }

    public static class PublisherSettingsExtensions
    {
        public static T UseContractsFromAssemblies<T>(this T self, Assembly[] assembliesContainingContracts, Type[] contracts = null)
            where T : ICanConfigureSerializer
        {
            var builder = self as ISettingsBuilder;
            if (builder.Container.IsRegistered<ISerializer>())
                throw new InvalidOperationException("Protoreg serializer is already initialized.");

            var protoreg = new Elders.Protoreg.ProtoRegistration();
            protoreg.RegisterAssembly(typeof(EventBatchWraper));
            protoreg.RegisterAssembly(typeof(IMessage));

            foreach (var ass in assembliesContainingContracts)
            {
                protoreg.RegisterAssembly(ass);
            }

            if (contracts != null)
                contracts.ToList().ForEach(c => protoreg.RegisterCommonType(c));

            var ser = new ProtoregSerializer(protoreg);
            ser.Build();
            builder.Container.RegisterSingleton<ISerializer>(() => ser);
            return self;
        }
    }
}