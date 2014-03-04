using System;
using System.Collections.Generic;

namespace NMSD.Cronus.DomainModelling
{
    public interface IValueObject<T> : IEqualityComparer<T>, IEquatable<T>
    {

    }
}
