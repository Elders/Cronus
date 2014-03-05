using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using NHibernate;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Messaging.MessageHandleScope;
using NMSD.Cronus.Pipelining;
using NMSD.Cronus.Pipelining.InMemory.Config;
using NMSD.Cronus.Pipelining.Transport.Config;
using NMSD.Cronus.Sample.Collaboration;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;
using NMSD.Cronus.Sample.Collaboration.Projections;

namespace NMSD.Cronus.Sample.Player
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

    class Program
    {
        static void Main(string[] args)
        {
            var sf = BuildSessionFactory();
            var cfg = new CronusConfiguration();
            cfg.ConfigureEventStore<MssqlEventStore>(eventStore =>
            {
                eventStore.BoundedContext = "Collaboration";
                eventStore.ConnectionString = ConfigurationManager.ConnectionStrings["LaCore_Hyperion_Collaboration_EventStore"].ConnectionString;
                eventStore.AggregateStatesAssembly = Assembly.GetAssembly(typeof(CollaboratorState));
            });
            cfg.ConfigurePublisher<PipelinePublisher<ICommand>>("Collaboration", publisher =>
            {
                publisher.InMemory(t => t.UsePipelinePerApplication());
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(CreateNewCollaborator)) };
            });

            cfg.ConfigureConsumer<EndpointConsumer<ICommand>>("Collaboration", consumer =>
            {
                //consumer.UnitOfWorkFactory
                consumer.NumberOfWorkers = 1;
                consumer.ScopeFactory = new ScopeFactory();
                consumer.ScopeFactory.CreateHandlerScope = () => new NHibernateHandlerScope(sf);
                consumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(CollaboratorAppService)), (type, context) =>
                    {
                        var handler = FastActivator.CreateInstance(type, null);
                        var nhHandler = handler as IHaveNhibernateSession;
                        if (nhHandler != null)
                        {
                            nhHandler.Session = context.HandlerScopeContext.Get<ISession>();
                        }
                        return handler;
                    });
                consumer.InMemory();
            })
            .Start();

            cfg.publishers["Collaboration"][typeof(ICommand)].Publish(new CreateNewCollaborator(new CollaboratorId(Guid.NewGuid()), "mynkow@gmail.com"));
        }

        static ISessionFactory BuildSessionFactory()
        {
            var typesThatShouldBeMapped = Assembly.GetAssembly(typeof(CollaboratorProjection)).GetExportedTypes().Where(t => t.Namespace.EndsWith("DTOs"));
            var cfg = new NHibernate.Cfg.Configuration();
            cfg = cfg.AddAutoMappings(typesThatShouldBeMapped);
            return cfg.BuildSessionFactory();
        }
    }
}