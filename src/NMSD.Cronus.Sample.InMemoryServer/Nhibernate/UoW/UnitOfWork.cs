using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NMSD.Cronus.UnitOfWork;

namespace NMSD.Cronus.Sample.Nhibernate.UoW
{
    public class NhibernateUnitOfWorkFactory : IScopeFactory
    {
        ISessionFactory nhSessionFactory;

        public NhibernateUnitOfWorkFactory(ISessionFactory nhSessionFactory)
        {
            this.nhSessionFactory = nhSessionFactory;
        }

        public IBatchScope NewBatchScope()
        {
            return new NullUnitOfWorkPerBatch();
        }

        public IMessageScope NewMessageScope()
        {
            return new NullUnitOfWorkPerMessage();
        }

        public IHandlerScope NewHandlerScope()
        {
            return new NhibernateUnitOfWork(nhSessionFactory);
        }
    }

    public class DependancyResolver : IDependancyResolver
    {
        ISession session;

        public DependancyResolver(ISession session)
        {
            this.session = session;
        }

        public T ResolveDependancies<T>(T instance)
        {
            var hassSession = instance as IHaveNhibernateSession;
            if (hassSession != null)
                hassSession.Session = session;
            return instance;
        }
    }

    public class NhibernateUnitOfWork : IHandlerScope
    {
        private ISession session;

        private ITransaction tx;

        private readonly ISessionFactory nhSessionFactory;

        public IMessageScope MessageScope { get; set; }

        public IDependancyResolver Resolver { get; set; }

        public NhibernateUnitOfWork(ISessionFactory nhSessionFactory)
        {
            this.nhSessionFactory = nhSessionFactory;
        }

        public void Begin()
        {
            session = nhSessionFactory.OpenSession();
            tx = session.BeginTransaction();
            Resolver = new DependancyResolver(session);
        }

        public void Commit()
        {
            tx.Commit();
        }

        public void Rollback()
        {
            tx.Rollback();
            tx.Dispose();
            session.Close();
        }

        public void Dispose()
        {
            tx.Dispose();
            session.Dispose();
        }
    }
}
