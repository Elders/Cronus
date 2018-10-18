using Elders.Cronus.Discoveries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus
{
    public static class CronusServiceCollectionExtensions
    {
        public static IServiceCollection AddCronus(this IServiceCollection services, IConfiguration configuration)
        {
            var discoveryFinder = new DiscoveryScanner(new CronusServicesProvider(services), configuration);
            discoveryFinder.Discover();

            return services;
        }
    }
}
