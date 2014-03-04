using System.Configuration;
using System.Linq;
using System.Reflection;
using NHibernate;
using NMSD.Cronus.Hosts;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
using NMSD.Cronus.Sample.Collaboration.Projections;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Events;
using NMSD.Cronus.Sample.Nhibernate.UoW;
using NMSD.Cronus.UnitOfWork;
using NMSD.Cronus.Userfull;

namespace NMSD.Cronus.Sample.Handlers
{
    class Program
    {
        private static CronusHost host;
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            UseCronusHost();
            System.Console.WriteLine("Started event handlers");
            System.Console.ReadLine();
            host.Release();
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
            //var connectionString = ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
            //DatabaseManager.DeleteDatabase(connectionString);
            //DatabaseManager.CreateDatabase(connectionString, "use_default", true);
            //DatabaseManager.EnableSnapshotIsolation(connectionString);
            var sf = BuildSessionFactory();
            var uow = new NhibernateUnitOfWorkFactory(sf);
            host = new CronusHost();
            host.UseEventHandlersPipelinePerApplication();
            host.UseEventHandlerPerEndpoint();
            host.UseCommandPipelinePerApplication();
            host.UseRabbitMqTransport();
            host.ConfigureCommandPublisher(x =>
            {
                x.RegisterCommandsAssembly<RegisterNewUser>();
                x.RegisterCommandsAssembly<CreateNewCollaborator>();
            });
            host.BuildCommandPublisher();
            host.ConfigureEventHandlersConsumer(cfg =>
            {
                //cfg.SetUnitOfWorkFacotry(uow);
                cfg.RegisterEventsAssembly(Assembly.GetAssembly(typeof(NewCollaboratorCreated)));
                cfg.RegisterEventsAssembly(Assembly.GetAssembly(typeof(NewUserRegistered)));
                cfg.SetEventHandlersAssembly(Assembly.GetAssembly(typeof(CollaboratorProjection)));
            });
            host.BuildSerializer();
            host.HostEventHandlerConsumers(5);
        }
    }
}
