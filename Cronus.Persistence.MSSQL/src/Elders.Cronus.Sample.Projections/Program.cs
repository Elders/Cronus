using System;
using System.Linq;
using System.Reflection;
using NHibernate;
using NHibernate.Impl;
using NHibernate.Mapping.ByCode;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Transport.RabbitMQ.Config;
using Elders.Cronus.Sample.Collaboration;
using Elders.Cronus.Sample.Collaboration.Projections;
using Elders.Cronus.Sample.Collaboration.Users.Commands;
using Elders.Cronus.Sample.Collaboration.Users.DTOs;
using Elders.Cronus.Sample.CommonFiles;
using Elders.Cronus.Sample.Handlers.Nhibernate;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Commands;

namespace Elders.Cronus.Sample.Handlers
{
    class Program
    {
        static CronusHost host;
        public static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var sf = BuildSessionFactory();
            var cfg = new CronusConfiguration();

            const string Collaboration = "Collaboration";
            cfg.PipelineCommandPublisher(publisher =>
            {
                publisher.UseRabbitMqTransport();
                publisher.MessagesAssemblies = new Assembly[] { Assembly.GetAssembly(typeof(RegisterAccount)), Assembly.GetAssembly(typeof(CreateUser)) };
            });
            cfg.ConfigureConsumer<EndpointProjectionConsumableSettings>(Collaboration, consumer =>
            {
                consumer.ConsumerBatchSize = 100;
                consumer.SetNumberOfWorkers(2);
                consumer.ScopeFactory.CreateBatchScope = () => new BatchScope(sf);
                consumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserProjection)), (type, context) =>
                    {
                        var handler = FastActivator.CreateInstance(type, null);
                        var nhHandler = handler as IHaveNhibernateSession;
                        if (nhHandler != null)
                        {
                            nhHandler.Session = context.BatchScopeContext.Get<Lazy<ISession>>().Value;
                        }
                        return handler;
                    });
                consumer.UseRabbitMqTransport();
            })
            .Build();

            host = new CronusHost(cfg);
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