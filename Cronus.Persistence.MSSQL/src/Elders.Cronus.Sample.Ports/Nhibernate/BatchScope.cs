using System;
using NHibernate;
using Elders.Cronus.UnitOfWork;

namespace Elders.Cronus.Sample.Ports.Nhibernate
{
    public class BatchUnitOfWork : IBatchUnitOfWork
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(BatchUnitOfWork));

        private readonly ISessionFactory sessionFactory;
        private ISession session;
        private ITransaction transaction;

        public BatchUnitOfWork(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void Begin()
        {
            Lazy<ISession> lazySession = new Lazy<ISession>(() =>
            {
                if (session != null)
                {
                    int breakPoint = 0;
                    breakPoint++;
                }
                session = sessionFactory.OpenSession();

                transaction = session.BeginTransaction();
                return session;
            });
            Context.Set<Lazy<ISession>>(lazySession);
        }

        public void End()
        {
            if (session != null)
            {
                try { transaction.Commit(); }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    session.Clear();
                    session.Close();
                }
                session = null;
            }
        }

        public IUnitOfWorkContext Context { get; set; }
    }
}
