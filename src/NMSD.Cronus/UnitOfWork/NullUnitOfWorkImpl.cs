using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMSD.Cronus.UnitOfWork
{
    public class NullUnitOfWorkFactory : IUnitOfWorkFactory
    {

        public IUnitOfWorkPerBatch NewBatch()
        {
            return new NullUnitOfWorkPerBatch();
        }

        public IUnitOfWorkPerMessage NewMessage()
        {
            return new NullUnitOfWorkPerMessage();
        }

        public IUnitOfWorkPerHandler NewHandler()
        {
            return new NullUnitOfWorkPerHandler();
        }
    }

    public class NullUnitOfWorkPerHandler : IUnitOfWorkPerHandler
    {
        public NullUnitOfWorkPerHandler()
        {
            Resolver = new NullDependancyResolver();
        }
        public IUnitOfWorkPerMessage UoWMessage { get; set; }

        public IDependancyResolver Resolver { get; set; }

        public void Begin() { }

        public void Commit() { }

        public void Rollback() { }

        public void Dispose() { }
    }

    public class NullUnitOfWorkPerMessage : IUnitOfWorkPerMessage
    {
        public NullUnitOfWorkPerMessage()
        {
            Resolver = new NullDependancyResolver();
        }
        public IUnitOfWorkPerBatch UoWBatch { get; set; }

        public IDependancyResolver Resolver { get; set; }

        public void Begin() { }

        public void Commit() { }

        public void Rollback() { }

        public void Dispose() { }
    }

    public class NullUnitOfWorkPerBatch : IUnitOfWorkPerBatch
    {
        public IDependancyResolver Resolver { get; set; }

        public void Begin()
        {
            Resolver = new NullDependancyResolver();
        }

        public void Commit() { }

        public void Rollback() { }

        public void Dispose() { }
    }

    public class NullDependancyResolver : IDependancyResolver
    {

        public T ResolveDependancies<T>(T instance)
        {
            return instance;
        }
    }

}
