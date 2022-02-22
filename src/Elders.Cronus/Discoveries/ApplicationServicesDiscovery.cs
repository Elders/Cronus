using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.EventStore;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public abstract class HandlersDiscovery<T> : DiscoveryBase<T>
    {
        protected override DiscoveryResult<T> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<T>(DiscoverHandlers(context));
        }

        protected virtual IEnumerable<DiscoveredModel> DiscoverHandlers(DiscoveryContext context)
        {
            var foundTypes = context.FindService<T>();
            foreach (var type in foundTypes)
            {
                yield return new DiscoveredModel(type, type, ServiceLifetime.Transient);
            }

            yield return new DiscoveredModel(typeof(TypeContainer<T>), new TypeContainer<T>(foundTypes));
            yield return new DiscoveredModel(typeof(IHandlerFactory), provider => new DefaultHandlerFactory(type => provider.GetRequiredService(type)), ServiceLifetime.Transient);
        }
    }

    public class ApplicationServicesDiscovery : HandlersDiscovery<IApplicationService> { }

    public class SystemApplicationServicesDiscovery : HandlersDiscovery<ISystemAppService> { }

    public class PortsDiscovery : HandlersDiscovery<IPort> { }

    public class SystemPortsDiscovery : HandlersDiscovery<ISystemPort> { }

    public class SagasDiscovery : HandlersDiscovery<ISaga> { }

    public class SystemSagasDiscovery : HandlersDiscovery<ISystemSaga> { }

    public class GatewaysDiscovery : HandlersDiscovery<IGateway> { }

    public class TriggersDiscovery : HandlersDiscovery<ITrigger> { }

    public class SystemTriggersDiscovery : HandlersDiscovery<ISystemTrigger> { }

    public class MigrationsDiscovery : HandlersDiscovery<IMigrationHandler>
    {
        protected override DiscoveryResult<IMigrationHandler> DiscoverFromAssemblies(DiscoveryContext context)
        {
            IEnumerable<DiscoveredModel> models =
                DiscoverHandlers(context)
                .Concat(DiscoverCustomLogic(context));

            return new DiscoveryResult<IMigrationHandler>(models);
        }

        protected virtual IEnumerable<DiscoveredModel> DiscoverCustomLogic(DiscoveryContext context)
        {
            yield return new DiscoveredModel(typeof(IMigrationCustomLogic), typeof(NoCustomLogic), ServiceLifetime.Transient)
            {
                CanOverrideDefaults = true
            };
        }
    }

    public class NoCustomLogic : IMigrationCustomLogic
    {
        public void OnAggregateCommit(AggregateCommit migratedAggregateCommit) { }
    }
}
