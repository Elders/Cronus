using System.Reflection;
using NMSD.Cronus;
using NMSD.Cronus.Commanding;
using NMSD.Cronus.Eventing;
using NMSD.Cronus.UnitOfWork;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
using NMSD.Cronus.Sample.Collaboration.Projections;
using NMSD.Protoreg;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Events;
using NMSD.Cronus.RabbitMQ;
using NMSD.Cronus.Transports.Conventions;
using NMSD.Cronus.Transports.RabbitMQ;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Hosts;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;
using NHibernate;
using System.Linq;
using System.Configuration;
using NMSD.Cronus.Sample.Nhibernate.UoW;
namespace NMSD.Cronus.Sample.Handlers
{
    class Program
    {
        private static CronusHost host;
        static void Main(string[] args)
        {
            //log4net.Config.XmlConfigurator.Configure();

            //var protoRegistration = new ProtoRegistration();
            //protoRegistration.RegisterAssembly<NewCollaboratorCreated>();
            //protoRegistration.RegisterAssembly<NewUserRegistered>();
            //protoRegistration.RegisterAssembly<Wraper>();
            //ProtoregSerializer serializer = new ProtoregSerializer(protoRegistration);
            //serializer.Build();

            //var rabbitMqSessionFactory = new RabbitMqSessionFactory();
            //var session = rabbitMqSessionFactory.OpenSession();
            //var commandPublisher = new CommandPublisher(new CommandPipelinePerApplication(), new RabbitMqPipelineFactory(session), serializer);

            //var eventConsumer = new EventConsumer(new EventHandlerPerEndpoint(new EventHandlersPipelinePerApplication()), new RabbitMqEndpointFactory(session), serializer, commandPublisher);
            //eventConsumer.UnitOfWorkFactory = new NullUnitOfWorkFactory();
            //eventConsumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(CollaboratorProjection)));
            //eventConsumer.Start(2);
            UseCronusHost();
            System.Console.ReadLine();
            //session.Close();
        }
        static ISessionFactory BuildSessionFactory()
        {
            var typesThatShouldBeMapped = Assembly.GetAssembly(typeof(CollaboratorProjection)).GetExportedTypes().Where(t => t.Namespace.EndsWith("DTOs"));
            var cfg = new NHibernate.Cfg.Configuration();
            cfg = cfg.AddAutoMappings(typesThatShouldBeMapped);
            cfg.CreateDatabaseTables();
            return cfg.BuildSessionFactory();
        }
        static void UseCronusHost()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
            DatabaseManager.DeleteDatabase(connectionString);
            DatabaseManager.CreateDatabase(connectionString, "use_default", true);
            DatabaseManager.EnableSnapshotIsolation(connectionString);
            var sf = BuildSessionFactory();
            var uow = new NhibernateUnitOfWorkFactory(sf);
            host = new CronusHost();
            host.UseRabbitMqTransport();
            host.UseEventHandlersPipelinePerApplication();
            host.UseEventHandlerPerEndpoint();
            host.UseCommandPipelinePerApplication();
            host.ConfigureCommandPublisher(x =>
            {
                x.RegisterCommandsAssembly<RegisterNewUser>();
                x.RegisterCommandsAssembly<CreateNewCollaborator>();
            });
            host.BuildCommandPublisher();
            host.ConfigureEventHandlersConsumer(cfg =>
            {
                cfg.SetUnitOfWorkFacotry(uow);
                cfg.RegisterEventsAssembly(Assembly.GetAssembly(typeof(NewCollaboratorCreated)));
                cfg.RegisterEventsAssembly(Assembly.GetAssembly(typeof(NewUserRegistered)));
                cfg.SetEventHandlersAssembly(Assembly.GetAssembly(typeof(CollaboratorProjection)));
            });
            host.BuildSerializer();
            host.HostEventHandlerConsumers(1);
        }
    }
}
