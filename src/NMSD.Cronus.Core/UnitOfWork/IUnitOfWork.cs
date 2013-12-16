using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMSD.Cronus.Core.UnitOfWork
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWorkPerBatch NewBatch();
        IUnitOfWorkPerMessage NewMessage();
        IUnitOfWorkPerHandler NewHandler();
    }
    
    public interface IDependancyResolver
    {
        T ResolveDependancies<T>(T instance);
    }
    public interface IUnitOfWork : IDisposable
    {
        IDependancyResolver Resolver { get; set; }
        void Begin();
        void Commit();
        void Rollback();
    }
    public interface IUnitOfWorkPerHandler : IUnitOfWork
    {
        IUnitOfWorkPerMessage UoWMessage { get; set; }
    }
    public interface IUnitOfWorkPerBatch : IUnitOfWork
    {

    }
    public interface IUnitOfWorkPerMessage : IUnitOfWork
    {
        IUnitOfWorkPerBatch UoWBatch { get; set; }
    }

    
}
