using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading;
using NHibernate;
using NMSD.Cronus.Commanding;
using NMSD.Cronus.Hosts;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
using NMSD.Cronus.Sample.Collaboration.Projections;
using NMSD.Cronus.Sample.IdentityAndAccess.Users;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Events;
using NMSD.Cronus.Sample.Nhibernate.UoW;
using NMSD.Cronus.UnitOfWork;
using NMSD.Cronus.Userfull;

namespace NMSD.Cronus.Sample.InMemoryServer
{
    class Program
    {
        static CommandPublisher commandPublisher;

        public static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            UseInMemoryCommandPublisherHost();
            UseInMemoryCommandConsumerHost();
            UseEventStoreHost();
            UseCronusHost();

            HostUI(0);

            Console.ReadLine();
        }

        private static void HostUI(int messageDelayInMilliseconds = 0, int batchSize = 1)
        {

            for (int i = 0; i > -1; i++)
            {
                if (messageDelayInMilliseconds == 0)
                {
                    PublishCommands();
                }
                else
                {
                    for (int j = 0; j < batchSize; j++)
                    {
                        PublishCommands();
                    }

                    Thread.Sleep(messageDelayInMilliseconds);
                }
            }
        }

        private static void PublishCommands()
        {
            UserId userId = new UserId(Guid.NewGuid());
            var email = "mynkow@gmail.com";
            commandPublisher.Publish(new RegisterNewUser(userId, email));
            Thread.Sleep(1000);

            //commandPublisher.Publish(new ChangeUserEmail(userId, email, "newEmail@gmail.com"));
        }

        static void UseInMemoryCommandPublisherHost()
        {
            CronusHost host = new CronusHost();
            host.UseCommandPipelinePerApplication();
            host.ConfigureCommandPublisher(cfg => cfg.RegisterCommandsAssembly<RegisterNewUser>());
            host.UseInMemoryTransport();
            host.BuildSerializer();
            host.BuildCommandPublisher();
            commandPublisher = host.CommandPublisher;
        }

        static void UseInMemoryCommandConsumerHost()
        {
            var host = new CronusHost();
            host.UseCommandPipelinePerApplication();
            host.UseCommandHandlersPerBoundedContext();
            host.UseInMemoryTransport();
            host.ConfigureCommandConsumer(cfg =>
            {
                cfg.SetEventStoreConnectionString(ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString);
                cfg.SetUnitOfWorkFacotry(new NullUnitOfWorkFactory());
                cfg.SetAggregateStatesAssembly(Assembly.GetAssembly(typeof(CollaboratorState)));
                cfg.SetEventsAssembly(Assembly.GetAssembly(typeof(NewCollaboratorCreated)));
                cfg.SetCommandsAssembly(Assembly.GetAssembly(typeof(CreateNewCollaborator)));
                cfg.SetCommandHandlersAssembly(Assembly.GetAssembly(typeof(CollaboratorAppService)));
            });
            host.ConfigureCommandConsumer(cfg =>
            {
                cfg.SetEventStoreConnectionString(ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString);
                cfg.SetUnitOfWorkFacotry(new NullUnitOfWorkFactory());
                cfg.SetAggregateStatesAssembly(Assembly.GetAssembly(typeof(UserState)));
                cfg.SetEventsAssembly(Assembly.GetAssembly(typeof(NewUserRegistered)));
                cfg.SetCommandsAssembly(Assembly.GetAssembly(typeof(RegisterNewUser)));
                cfg.SetCommandHandlersAssembly(Assembly.GetAssembly(typeof(UserAppService)));
            });
            host.BuildSerializer();
            host.HostCommandConsumers(1);
        }

        static void UseEventStoreHost()
        {
            var host = new CronusHost();
            host.UseEventHandlersPipelinePerApplication();
            host.UseEventStorePipelinePerApplication();
            host.UseEventStorePerBoundedContext();
            host.UseInMemoryTransport();
            host.ConfigureEventStoreConsumer(cfg =>
            {
                cfg.SetEventStoreConnectionString(ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString);
                cfg.SetUnitOfWorkFacotry(new NullUnitOfWorkFactory());
                cfg.SetAggregateStatesAssembly(Assembly.GetAssembly(typeof(CollaboratorState)));
                cfg.SetEventsAssembly(typeof(NewCollaboratorCreated));
            });
            host.ConfigureEventStoreConsumer(cfg =>
            {
                cfg.SetEventStoreConnectionString(ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString);
                cfg.SetUnitOfWorkFacotry(new NullUnitOfWorkFactory());
                cfg.SetAggregateStatesAssembly(Assembly.GetAssembly(typeof(UserState)));
                cfg.SetEventsAssembly(typeof(NewUserRegistered));
            });
            host.BuildSerializer();
            host.HostEventStoreConsumers(1);
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
            var host = new CronusHost();
            host.UseEventHandlersPipelinePerApplication();
            host.UseEventHandlerPerEndpoint();
            host.UseCommandPipelinePerApplication();
            host.UseInMemoryTransport();
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