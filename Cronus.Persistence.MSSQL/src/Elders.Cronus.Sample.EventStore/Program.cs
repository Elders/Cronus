using System.Configuration;
using System.Reflection;
using Elders.Cronus.Persistence.MSSQL;
using Elders.Cronus.Persistence.MSSQL.Config;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Sample.Collaboration.Users;
using Elders.Cronus.Sample.Collaboration.Users.Commands;
using Elders.Cronus.Sample.Collaboration.Users.Events;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Commands;

namespace Elders.Cronus.Sample.EventStore
{
    class Program
    {
        static CronusHost host;
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            DatabaseManager.DeleteDatabase(ConfigurationManager.ConnectionStrings["cronus_es"].ConnectionString);
            UseCronusHost();
            System.Console.WriteLine("Started Event store");
            System.Console.ReadLine();
            host.Stop();
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

            cfg
                .UseMsSqlEventStore(eventStore => eventStore
                    .SetConnectionStringName("cronus_es")
                    .SetAggregateStatesAssembly(typeof(AccountState))
                    .WithNewStorageIfNotExists())
                .UseDefaultMsSqlEventStoreHost("IdentityAndAccess", typeof(RegisterAccount));

            cfg.UseMsSqlEventStore(eventStore => eventStore
                    .SetConnectionStringName("cronus_es")
                    .SetAggregateStatesAssembly(typeof(UserState))
                    .WithNewStorageIfNotExists())
                .UseDefaultMsSqlEventStoreHost("Collaboration", typeof(UserCreated));

            host = new CronusHost(cfg.GetInstance());
            host.Start();
        }

    }
}
