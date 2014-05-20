using System;
using System.Linq;
using System.Reflection;
using NHibernate;
using NHibernate.Mapping.ByCode;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Transport.RabbitMQ.Config;
using Elders.Cronus.Sample.Collaboration;
using Elders.Cronus.Sample.Collaboration.Projections;
using Elders.Cronus.Sample.Collaboration.Users.DTOs;
using Elders.Cronus.Sample.CommonFiles;
using Elders.Cronus.Sample.Handlers.Nhibernate;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Commands;
using Elders.Cronus.Sample.Collaboration.Users.Commands;
using Elders.Cronus.Messaging.MessageHandleScope;

namespace Elders.Cronus.Sample.Handlers
{
    class Program
    {
        static CronusHost host;
        public static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var sf = BuildSessionFactory();
            var cfg = new CronusSettings()
                .UseContractsFromAssemblies(new Assembly[] { Assembly.GetAssembly(typeof(RegisterAccount)), Assembly.GetAssembly(typeof(CreateUser)) })
                .WithDefaultPublishers()
                .UseProjectionConsumable("Collaboration", consumable => consumable
                    .SetNumberOfConsumers(2)
                    .UseRabbitMqTransport()
                    .EventConsumer(c => c
                        .UseEventHandler(h => h
                            .UseScopeFactory(new ScopeFactory() { CreateBatchScope = () => new BatchScope(sf) })
                            .SetConsumerBatchSize(100)
                            .RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserProjection)), (type, context) =>
                            {
                                return FastActivator.CreateInstance(type)
                                    .AssignPropertySafely<IHaveNhibernateSession>(x => x.Session = context.BatchScopeContext.Get<Lazy<ISession>>().Value);
                            }))));

            host = new CronusHost(cfg.GetInstance());
            host.Start();

            Console.WriteLine("Projections started");
            Console.ReadLine();
            host.Stop();
        }

        static ISessionFactory BuildSessionFactory()
        {
            var typesThatShouldBeMapped = Assembly.GetAssembly(typeof(UserProjection)).GetExportedTypes().Where(t => t.Namespace.EndsWith("DTOs"));
            var cfg = new NHibernate.Cfg.Configuration();
            Action<ModelMapper> customMappings = modelMapper =>
            {
                modelMapper.Class<User>(mapper =>
                {
                    mapper.Property(pr => pr.Email, prmap => prmap.Unique(true));
                });
            };

            cfg = cfg.AddAutoMappings(typesThatShouldBeMapped, customMappings);
            cfg.CreateDatabase_AND_OVERWRITE_EXISTING_DATABASE();
            return cfg.BuildSessionFactory();
        }
    }
}