using Elders.Cronus.AtomicAction;
using Elders.Cronus.AtomicAction.InMemory;
//using Elders.Cronus.AtomicAction.InMemory;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.IntegrityValidation;
using Elders.Cronus.IocContainer;
using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.EventStore.InMemory.Config
{
    public static class InMemoryEventStoreExtensions
    {
        public static T UseInMemoryEventStore<T>(this T self) where T : IConsumerSettings<ICommand>
        {
            InMemoryEventStoreSettings settings = new InMemoryEventStoreSettings(self);
            (settings as ISettingsBuilder).Build();
            return self;
        }
    }

    public interface IInMemoryEventStoreSettings : ISettingsBuilder { }

    public class InMemoryEventStoreSettings : SettingsBuilder, IInMemoryEventStoreSettings
    {
        public InMemoryEventStoreSettings(ISettingsBuilder settingsBuilder)
            : base(settingsBuilder)
        {
        }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;

            builder.Container.RegisterSingleton<InMemoryEventStoreStorage>(() => new InMemoryEventStoreStorage());
            builder.Container.RegisterSingleton<IAggregateRootAtomicAction>(() => new InMemoryAggregateRootAtomicAction());
            builder.Container.RegisterSingleton<IEventStore>(() => new InMemoryEventStore(builder.Container.Resolve<InMemoryEventStoreStorage>()));
            builder.Container.RegisterSingleton<IIntegrityPolicy<EventStream>>(() => new EventStreamIntegrityPolicy());
            builder.Container.RegisterSingleton<IAggregateRepository>(() => new AggregateRepository(builder.Container.Resolve<IEventStore>(), builder.Container.Resolve<IAggregateRootAtomicAction>(), builder.Container.Resolve<IIntegrityPolicy<EventStream>>()));
            builder.Container.RegisterSingleton<IEventStorePlayer>(() => new InMemoryEventStorePlayer(builder.Container.Resolve<InMemoryEventStoreStorage>()));
            builder.Container.RegisterSingleton<IEventStoreStorageManager>(() => new InMemoryEventStoreStorageManager());
        }
    }
}
