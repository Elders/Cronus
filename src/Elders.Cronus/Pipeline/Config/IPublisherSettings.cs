using System;
using System.Linq;
using System.Reflection;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Serializer;
using Elders.Cronus.Serializer.Protoreg;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IPipelinePublisherSettings<TContract> : IHaveTransport<IPipelineTransport>, ISettingsBuilder<IPublisher<TContract>>, IHavePipelineSettings<TContract>, IHaveSerializer
        where TContract : IMessage
    {

    }

    public static class PublisherSettingsExtensions
    {
        internal static void CopySerializerTo(this IHaveSerializer self, IHaveSerializer destination)
        {
            destination.Serializer = self.Serializer;
        }

        internal static void CopyContainerTo(this IHaveContainer self, IHaveContainer destination)
        {
            destination.Container = self.Container;
        }

        static ISerializer serializer = null;

        public static T UseContractsFromAssemblies<T>(this T self, Assembly[] assembliesContainingContracts, Type[] contracts = null)
            where T : IHaveSerializer, ICanConfigureSerializer
        {
            if (serializer != null)
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
            self.Serializer = ser;
            serializer = ser;
            return self;
        }
    }
}