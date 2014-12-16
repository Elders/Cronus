using Elders.Cronus.Pipeline.CircuitBreaker;
using Elders.Cronus.IocContainer;

namespace Elders.Cronus.Pipeline.Config
{
    public static class EndpointCircuitBreakerExtensions
    {
        public static T WithDefaultCircuitBreaker<T>(this T self) where T : ICanConfigureCircuitBreaker
        {
            self.Container.RegisterSingleton<IEndpontCircuitBreakerFactrory>(() => new DefaultCircuitBreakerFactory(), self.Name);
            return self;
        }
    }
}
