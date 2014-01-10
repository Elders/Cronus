using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMSD.Cronus.UnitOfWork
{
    public interface IDependancyResolver
    {
        T ResolveDependancies<T>(T instance);
    }
}
