using System;
using System.Linq;
using System.Reflection;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.UnitOfWork;
using Elders.Cronus.Persistence.Cassandra.Config;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Transport.InMemory.Config;
using Elders.Cronus.Sample.Collaboration;
using Elders.Cronus.Sample.Collaboration.Users;
using Elders.Cronus.Sample.Collaboration.Users.Events;
using Elders.Cronus.Sample.Collaboration.Users.Projections;
using Elders.Cronus.Sample.CommonFiles;
using NHibernate;

namespace Elders.Cronus.Sample.Player
{
    public class HandlerScope : IHandlerUnitOfWork
    {
        private readonly ISessionFactory sessionFactory;
        private ISession session;
        private ITransaction transaction;

        public HandlerScope(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void Begin()
        {
            Context = new UnitOfWorkContext();

            Lazy<ISession> lazySession = new Lazy<ISession>(() =>
            {
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
                transaction.Commit();
                session.Clear();
                session.Close();
            }
        }

        public IUnitOfWorkContext Context { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var sf = BuildSessionFactory();

            var cfg = new CronusSettings()
                .UseContractsFromAssemblies(new Assembly[] { Assembly.GetAssembly(typeof(UserState)), Assembly.GetAssembly(typeof(UserCreated)) })
                .WithDefaultPublishersInMemory();

            cfg
                .UseCassandraEventStore(eventStore => eventStore
                    .SetConnectionStringName("cronus_es")
                    .SetAggregateStatesAssembly(typeof(UserState)))
                .UseProjectionConsumable("Collaboration", consumable => consumable
                    .SetNumberOfConsumers(1)
                    .UseInMemoryTransport()
                    .EventConsumer(c => c
                        .UseEventHandler(h => h
                            .UseUnitOfWork(new UnitOfWorkFactory() { CreateHandlerUnitOfWork = () => new HandlerScope(sf) })
                            .RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserProjection)), (type, context) =>
                            {
                                return FastActivator.CreateInstance(type)
                                    .AssignPropertySafely<IHaveNhibernateSession>(x => x.Session = context.HandlerContext.Get<Lazy<ISession>>().Value);
                            }))));

            new CronusPlayer(cfg.GetInstance()).Replay();
        }

        static ISessionFactory BuildSessionFactory()
        {
            var typesThatShouldBeMapped = Assembly.GetAssembly(typeof(UserProjection)).GetExportedTypes().Where(t => t.Namespace.EndsWith("DTOs"));
            var cfg = new NHibernate.Cfg.Configuration();
            cfg = cfg.AddAutoMappings(typesThatShouldBeMapped);
            cfg.Configure();
            cfg.CreateDatabase_AND_OVERWRITE_EXISTING_DATABASE();
            return cfg.BuildSessionFactory();
        }
    }
}