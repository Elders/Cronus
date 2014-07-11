using System.Reflection;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Sample.Collaboration.Users;
using Elders.Cronus.Sample.Collaboration.Users.Commands;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Commands;
using Elders.Cronus.Persistence.MSSQL.Config;
using System;
using Elders.Cronus.Persistence.MSSQL;

namespace Elders.Cronus.Sample.ApplicationService
{
    class Program
    {
        static CronusHost host;
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            DatabaseManager.DeleteDatabase("Server=.;Database=CronusES;User Id=sa;Password=sa;");
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
                .WithDefaultPublishersWithRabbitMq();

            const string IAA = "IdentityAndAccess";
            cfg.UseMsSqlEventStore(eventStore => eventStore
                    .SetConnectionStringName("cronus_es")
                    .SetAggregateStatesAssembly(typeof(AccountState))
                    .WithNewStorageIfNotExists())
                .UseDefaultCommandsHostWithRabbitMq(IAA, typeof(AccountAppService), (type, context) =>
                {
                    return FastActivator.CreateInstance(type)
                        .AssignPropertySafely<IAggregateRootApplicationService>(x => x.Repository = context.BatchContext.Get<Lazy<IAggregateRepository>>().Value);
                });

            const string Collaboration = "Collaboration";
            cfg.UseMsSqlEventStore(eventStore => eventStore
                    .SetConnectionStringName("cronus_es")
                    .SetAggregateStatesAssembly(typeof(UserState))
                    .WithNewStorageIfNotExists())
                .UseDefaultCommandsHostWithRabbitMq(Collaboration, typeof(UserAppService), (type, context) =>
                {
                    return FastActivator.CreateInstance(type)
                        .AssignPropertySafely<IAggregateRootApplicationService>(x => x.Repository = context.BatchContext.Get<Lazy<IAggregateRepository>>().Value);
                });

            host = new CronusHost(cfg.GetInstance());
            host.Start();
        }
    }
}