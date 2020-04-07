using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.Discoveries;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Multitenancy
{
    public class MultitenancyDiscovery : DiscoveryBase<IMultitenancy>
    {
        protected override DiscoveryResult<IMultitenancy> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<IMultitenancy>(GetModels(context), services => services.AddOptions<TenantsOptions, TenantsOptionsProvider>());
        }

        IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
        {
            yield return new DiscoveredModel(typeof(ITenantResolver), typeof(TenantResolver), ServiceLifetime.Singleton);

            var resolverTypes = GetAllTypesImplementingOpenGenericType(context, typeof(ITenantResolver<>));

            foreach (var resolverType in resolverTypes)
            {
                var interfaces = resolverType.GetInterfaces().Where(intf => typeof(ITenantResolver<>).IsAssignableFrom(intf.GetGenericTypeDefinition()));
                foreach (var interf in interfaces)
                {
                    yield return new DiscoveredModel(interf, resolverType, ServiceLifetime.Singleton);
                }
            }
        }

        private IEnumerable<Type> GetAllTypesImplementingOpenGenericType(DiscoveryContext context, Type openGenericType)
        {
            var resolverTypes = context.Assemblies
                .SelectMany(asm => asm.GetLoadableTypes())
                .Where(type => type.IsAbstract == false && type.IsInterface == false && type.GetInterfaces()
                                                                                            .Where(candidate => candidate.IsGenericType)
                                                                                            .Any(candidate => openGenericType.IsAssignableFrom(candidate.GetGenericTypeDefinition())));

            return resolverTypes;
        }
    }
}
