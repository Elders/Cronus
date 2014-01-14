using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMSD.Cronus.DomainModelling
{
    public interface IValueObject<T> : IEqualityComparer<T>, IEquatable<T>
    {

    }
}
