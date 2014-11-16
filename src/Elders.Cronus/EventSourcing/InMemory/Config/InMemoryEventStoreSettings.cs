using Elders.Cronus.DomainModeling;
using Elders.Cronus.IocContainer;
using Elders.Cronus.Pipeline.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elders.Cronus.EventSourcing.InMemory.Config
{
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
            builder.Container.RegisterSingleton<IAggregateVersionService>(() => new InMemoryAggregateVersionService());
            builder.Container.RegisterSingleton<IEventStorePersister>(() => new InMemoryEventStorePersister(builder.Container.Resolve<InMemoryEventStoreStorage>()));
            builder.Container.RegisterSingleton<IAggregateRepository>(() => new InMemoryAggregateRepository(builder.Container.Resolve<IEventStorePersister>(), builder.Container.Resolve<InMemoryEventStoreStorage>(), builder.Container.Resolve<IAggregateVersionService>()));
            builder.Container.RegisterSingleton<IEventStorePlayer>(() => new InMemoryEventStorePlayer(builder.Container.Resolve<InMemoryEventStoreStorage>()));
            builder.Container.RegisterSingleton<IEventStoreStorageManager>(() => new InMemoryEventStoreStorageManager());
            builder.Container.RegisterSingleton<IEventStore>(() => new InMemoryEventStore(
                builder.Container.Resolve<IAggregateRepository>(),
                builder.Container.Resolve<IEventStorePersister>(),
                builder.Container.Resolve<IEventStorePlayer>(),
                builder.Container.Resolve<IEventStoreStorageManager>()
                ));
        }
    }
}