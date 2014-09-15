using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Pipeline.CircuitBreaker;
using Elders.Cronus.Pipeline.Transport;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IHaveCircuitBreaker
    {
        Lazy<IEndpontCircuitBreakerFactrory> CircuitBreakerFactory { get; set; }
    }

    public static class EndpointCircuitBreakerExtensions
    {
        public static T WithDefaultCircuitBreaker<T>(this T self) where T : IHaveCircuitBreaker, IHaveSerializer, IHaveTransport<IPipelineTransport>
        {

            self.CircuitBreakerFactory = new Lazy<IEndpontCircuitBreakerFactrory>(() => new DefaultCircuitBreakerFactory());
            return self;
        }

        public static T WithNoCircuitBreaker<T>(this T self) where T : IHaveCircuitBreaker
        {
            self.CircuitBreakerFactory = new Lazy<IEndpontCircuitBreakerFactrory>(() => new NoCircuitBreakerFactory());
            return self;
        }
    }
}
