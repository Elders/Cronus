using System;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IEndpointPostConsumeSettings : IEndpointPostConsumeBuilder
    {
        Func<IEndpointConsumerErrorStrategy> ErrorStrategy { get; set; }
        Func<IEndpointConsumerSuccessStrategy> SuccessStrategy { get; set; }

        Func<IEndpointPostConsume> PostConsumeInstance { get; set; }
    }

    public interface IEndpointPostConsumeBuilder
    {
        IEndpointPostConsume BuildPostConsumeActions();
    }
}