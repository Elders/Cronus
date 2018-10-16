using System.Collections.Generic;
using Elders.Cronus.MessageProcessing;
using Elders.Cronus.Middleware;
using Elders.Cronus.Pipeline.Config;
using Microsoft.Extensions.DependencyInjection;

namespace Elders.Cronus.Discoveries
{
    public class CronusMiddwaresDiscovery : DiscoveryBasedOnExecutingDirAssemblies<IMiddleware>
    {
        protected override DiscoveryResult<IMiddleware> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<IMiddleware>(GetModels());
        }

        IEnumerable<DiscoveredModel> GetModels()
        {
            yield return new DiscoveredModel(typeof(MessageHandlerMiddleware), typeof(MessageHandlerMiddleware), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(ISubscriptionMiddleware<>), typeof(SubscriprionMiddlewareFactory<>), ServiceLifetime.Transient);
        }
    }
}
