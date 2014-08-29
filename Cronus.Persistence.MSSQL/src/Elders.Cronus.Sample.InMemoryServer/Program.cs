using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Persistence.MSSQL.Config;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Sample.Collaboration;
using Elders.Cronus.Sample.Collaboration.Projections;
using Elders.Cronus.Sample.Collaboration.Users;
using Elders.Cronus.Sample.Collaboration.Users.Commands;
using Elders.Cronus.Sample.Collaboration.Users.Events;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Events;
using Elders.Cronus.Sample.CommonFiles;
using NHibernate;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Commands;
using Elders.Cronus.UnitOfWork;
using Elders.Cronus.Sample.Handlers.Nhibernate;

namespace Elders.Cronus.Sample.InMemoryServer
{
    class Program
    {
        public static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            var sf = BuildNHibernateSessionFactory();
            const string BC = "ContextlessDomain";//This can be fixed in futre versions.I just don't have time now. Log issue probably?
            var cfg = new CronusSettings()
                .UseContractsFromAssemblies(new Assembly[] { Assembly.GetAssembly(typeof(RegisterAccount)), Assembly.GetAssembly(typeof(CreateUser)) })
                  .UseMsSqlEventStore(eventStore => eventStore
                    .SetConnectionStringName("cronus_es")
                    .SetAggregateStatesAssembly(typeof(AccountState))
                    .WithNewStorageIfNotExists());

            var configurationInstance = cfg.WithDefaultPublishersInMemory(BC, new Assembly[] { typeof(AccountAppService).Assembly, typeof(UserAppService).Assembly, typeof(UserProjection).Assembly }, (type, context) =>
                     {
                         return FastActivator.CreateInstance(type)
                             .AssignPropertySafely<IAggregateRootApplicationService>(x => x.Repository = context.BatchContext.Get<Lazy<IAggregateRepository>>().Value)
                             .AssignPropertySafely<IPort>(x => x.CommandPublisher = (cfg as IHaveCommandPublisher).CommandPublisher.Value)
                             .AssignPropertySafely<IHaveNhibernateSession>(x => x.Session = context.BatchContext.Get<Lazy<ISession>>().Value);
                     },
                     new UnitOfWorkFactory() { CreateBatchUnitOfWork = () => new BatchUnitOfWork(sf) }
                     ).GetInstance();

            while (true)
            {
                configurationInstance.CommandPublisher.Publish(new RegisterAccount(new AccountId(Guid.NewGuid()), "awwwww@email.com"));
                Console.WriteLine("Sent");
                Console.ReadLine();
            }
        }


        static ISessionFactory BuildNHibernateSessionFactory()
        {
            var typesThatShouldBeMapped = Assembly.GetAssembly(typeof(UserProjection)).GetExportedTypes().Where(t => t.Namespace.EndsWith("DTOs"));
            var cfg = new NHibernate.Cfg.Configuration()
                .AddAutoMappings(typesThatShouldBeMapped)
                .Configure()
                .CreateDatabase();

            return cfg.BuildSessionFactory();
        }
    }
}