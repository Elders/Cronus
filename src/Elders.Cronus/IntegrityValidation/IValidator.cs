using System;

namespace Elders.Cronus.IntegrityValidation;

public interface IValidator<T> : IComparable<IValidator<T>>
{
    IValidatorResult Validate(T candidate);

    uint PriorityLevel { get; }
}
