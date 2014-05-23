using System;
using System.Reflection;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Persistence.Cassandra.Config;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Sample.Collaboration.Users;
using Elders.Cronus.Sample.Collaboration.Users.Events;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Events;

namespace Elders.Cronus.Sample.ApplicationService
{
    class Program
    {
        static CronusHost host;
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            UseCronusHostWithCassandraEventStore();
            System.Console.WriteLine("Started command handlers");
            System.Console.ReadLine();
            host.Stop();
            host = null;
        }

        static void UseCronusHostWithCassandraEventStore()
        {
            var cfg = new CronusSettings()
                .UseContractsFromAssemblies(new[] { Assembly.GetAssembly(typeof(AccountRegistered)), Assembly.GetAssembly(typeof(UserCreated)) })
                .WithDefaultPublishersWithRabbitMq();

            cfg.UseCassandraEventStore(eventStore => eventStore
                    .SetConnectionStringName("cronus_es")
                    .SetAggregateStatesAssembly(typeof(AccountState))
                    .WithNewStorageIfNotExists());

            cfg.UseDefaultCommandsHostWithRabbitMq("IdentityAndAccess", typeof(AccountAppService), (type, context) =>
                {
                    return FastActivator.CreateInstance(type)
                        .AssignPropertySafely<IAggregateRootApplicationService>(x => x.Repository = context.BatchScopeContext.Get<Lazy<IAggregateRepository>>().Value);
                });

            cfg.UseCassandraEventStore(eventStore => eventStore
                    .SetConnectionStringName("cronus_es")
                    .SetAggregateStatesAssembly(typeof(UserState))
                    .WithNewStorageIfNotExists());

            cfg.UseDefaultCommandsHostWithRabbitMq("Collaboration", typeof(UserAppService), (type, context) =>
                {
                    return FastActivator.CreateInstance(type)
                        .AssignPropertySafely<IAggregateRootApplicationService>(x => x.Repository = context.BatchScopeContext.Get<Lazy<IAggregateRepository>>().Value);
                });


            host = new CronusHost(cfg.GetInstance());
            host.Start();
        }
    }
}