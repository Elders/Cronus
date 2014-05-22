using System.Reflection;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Sample.Collaboration.Users;
using Elders.Cronus.Sample.Collaboration.Users.Commands;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Commands;
using Elders.Cronus.Persistence.MSSQL.Config;
using Elders.Cronus.Messaging.MessageHandleScope;
using System;

namespace Elders.Cronus.Sample.ApplicationService
{


    class Program
    {
        static CronusHost host;
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            UseCronusHost();
            System.Console.WriteLine("Started command handlers");
            System.Console.ReadLine();
            host.Stop();
            host = null;
        }

        static void UseCronusHost()
        {
            var cfg = new CronusSettings()
                .UseContractsFromAssemblies(new Assembly[] {
                    Assembly.GetAssembly(typeof(RegisterAccount)),
                    Assembly.GetAssembly(typeof(CreateUser)),
                    Assembly.GetAssembly(typeof(UserState)),
                    Assembly.GetAssembly(typeof(AccountState)) })
                .WithDefaultPublishers();

            const string IAA = "IdentityAndAccess";
            cfg.UseMsSqlEventStore(eventStore => eventStore
                    .SetConnectionStringName("cronus_es")
                    .SetAggregateStatesAssembly(typeof(AccountState))
                    .WithNewStorageIfNotExists())
                .UseDefaultCommandsHost(IAA, typeof(AccountAppService), (type, context) =>
                {
                    var handler = FastActivator.CreateInstance(type);
                    var repositoryHandler = handler as IAggregateRootApplicationService;
                    if (repositoryHandler != null)
                        repositoryHandler.Repository = context.BatchScopeContext.Get<Lazy<IAggregateRepository>>().Value;
                    return handler;
                });

            const string Collaboration = "Collaboration";
            cfg.UseMsSqlEventStore(eventStore => eventStore
                    .SetConnectionStringName("cronus_es")
                    .SetAggregateStatesAssembly(typeof(UserState))
                    .WithNewStorageIfNotExists())
                .UseDefaultCommandsHost(Collaboration, typeof(UserAppService), (type, context) =>
                {
                    var handler = FastActivator.CreateInstance(type);
                    var repositoryHandler = handler as IAggregateRootApplicationService;
                    if (repositoryHandler != null)
                        repositoryHandler.Repository = context.BatchScopeContext.Get<Lazy<IAggregateRepository>>().Value;
                    return handler;
                });

            host = new CronusHost(cfg.GetInstance());
            host.Start();
        }
    }
}