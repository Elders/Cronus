using System;
using System.Linq;
using System.Reflection;
using NHibernate;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging.MessageHandleScope;
using NMSD.Cronus.Persistence.MSSQL.Config;
using NMSD.Cronus.Pipeline.Config;
using NMSD.Cronus.Pipeline.Hosts;
using NMSD.Cronus.Pipeline.Transport.InMemory.Config;
using NMSD.Cronus.Sample.Collaboration;
using NMSD.Cronus.Sample.Collaboration.Projections;
using NMSD.Cronus.Sample.Collaboration.Users;
using NMSD.Cronus.Sample.Collaboration.Users.Events;
using NMSD.Cronus.Sample.CommonFiles;

namespace NMSD.Cronus.Sample.Player
{
    public class HandlerScope : IHandlerScope
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
            Context = new ScopeContext();

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

        public IScopeContext Context { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var sf = BuildSessionFactory();

            var cfg = new CronusConfiguration();
            cfg.ConfigureEventStore<MsSqlEventStoreSettings>(eventStore =>
            {
                eventStore
                    .SetConnectionStringName("cronus-es")
                    .SetAggregateStatesAssembly(Assembly.GetAssembly(typeof(UserState)));
            });
            cfg.PipelineEventPublisher(publisher =>
            {
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(UserCreated)) };
                publisher.UseTransport<InMemory>();
            });
            cfg.ConfigureConsumer<EndpointProjectionConsumableSettings>("Collaboration", consumer =>
            {
                consumer.ScopeFactory.CreateHandlerScope = () => new HandlerScope(sf);
                consumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserProjection)).GetExportedTypes().Where(x => !typeof(IPort).IsAssignableFrom(x)).ToArray(), (type, context) =>
                    {
                        var handler = FastActivator.CreateInstance(type, null);
                        var nhHandler = handler as IHaveNhibernateSession;
                        if (nhHandler != null)
                            nhHandler.Session = context.HandlerScopeContext.Get<Lazy<ISession>>().Value;
                        return handler;
                    });
                consumer.UseTransport<InMemory>();
            })
            .Build();

            new CronusPlayer(cfg).Replay();
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