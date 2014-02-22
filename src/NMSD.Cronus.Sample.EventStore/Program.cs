using System.Configuration;
using System.Reflection;
using NMSD.Cronus.Hosts;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
using NMSD.Cronus.Sample.IdentityAndAccess.Users;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Events;
using NMSD.Cronus.UnitOfWork;

namespace NMSD.Cronus.Sample.EventStore
{
    class Program
    {
        private static CronusHost host;
        static void Main(string[] args)
        {
            //log4net.Config.XmlConfigurator.Configure();
            UseCronusHost();
            System.Console.WriteLine("Started Event store");
            System.Console.ReadLine();
            host.Release();
        }

        static void UseCronusHost()
        {
            host = new CronusHost();
            host.UseEventHandlersPipelinePerApplication();
            host.UseEventStorePipelinePerApplication();
            host.UseEventStorePerBoundedContext();
            host.UseRabbitMqTransport();
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
    }
}
