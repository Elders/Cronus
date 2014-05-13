using NHibernate;
using Elders.Cronus.Messaging.MessageHandleScope;

namespace Elders.Cronus.Sample.InMemoryServer.Nhibernate
{
    public class NHibernateHandlerScope : IHandlerScope
    {
        private readonly ISessionFactory sessionFactory;
        private ISession session;
        private ITransaction transaction;

        public NHibernateHandlerScope(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void Begin()
        {
            session = sessionFactory.OpenSession();
            transaction = session.BeginTransaction();
            Context.Set<ISession>(session);
        }

        public void End()
        {
            transaction.Commit();
            session.Clear();
            session.Close();
        }

        public IScopeContext Context { get; set; }
    }
}
