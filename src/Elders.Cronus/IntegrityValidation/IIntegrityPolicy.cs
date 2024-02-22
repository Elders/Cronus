using System.Collections.Generic;

namespace Elders.Cronus.IntegrityValidation;

public interface IIntegrityPolicy<T>
{
    IEnumerable<IntegrityRule<T>> Rules { get; }

    IntegrityResult<T> Apply(T candidate);
}
