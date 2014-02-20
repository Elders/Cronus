using System.Configuration;
using System.Reflection;
using NMSD.Cronus.Hosts;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
using NMSD.Cronus.Sample.IdentityAndAccess.Users;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Events;
using NMSD.Cronus.UnitOfWork;

namespace NMSD.Cronus.Sample.ApplicationService
{
    class Program
    {
        static CronusHost host;
        static void Main(string[] args)
        {
            //log4net.Config.XmlConfigurator.Configure();

            UseCronusHost();
            System.Console.WriteLine("Started");
            System.Console.ReadLine();
            host.Release();
        }

        static void UseCronusHost()
        {
            host = new CronusHost();
            host.UseCommandPipelinePerApplication();
            host.UseCommandHandlersPerBoundedContext();
            host.UseRabbitMqTransport();
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
    }
}