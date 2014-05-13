using System;
using System.Collections.Generic;

namespace Elders.Cronus.DomainModelling
{
    public interface IValueObject<T> : IEqualityComparer<T>, IEquatable<T>
    {

    }
}
