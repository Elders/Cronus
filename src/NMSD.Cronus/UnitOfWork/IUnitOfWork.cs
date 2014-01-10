using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMSD.Cronus.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IDependancyResolver Resolver { get; set; }
        void Begin();
        void Commit();
        void Rollback();
    }
}
