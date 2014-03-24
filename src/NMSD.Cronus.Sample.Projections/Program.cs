using System;
using System.Linq;
using System.Reflection;
using NHibernate;
using NHibernate.Impl;
using NHibernate.Mapping.ByCode;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Pipeline.Config;
using NMSD.Cronus.Pipeline.Hosts;
using NMSD.Cronus.Pipeline.Transport.RabbitMQ.Config;
using NMSD.Cronus.Sample.Collaboration;
using NMSD.Cronus.Sample.Collaboration.Projections;
using NMSD.Cronus.Sample.Collaboration.Users.Commands;
using NMSD.Cronus.Sample.Collaboration.Users.DTOs;
using NMSD.Cronus.Sample.CommonFiles;
using NMSD.Cronus.Sample.Handlers.Nhibernate;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts.Commands;

namespace NMSD.Cronus.Sample.Handlers
{
    class Program
    {
        public static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var sf = BuildSessionFactory();
            var cfg = new CronusConfiguration();

            const string Collaboration = "Collaboration";
            cfg.PipelineCommandPublisher(publisher =>
            {
                publisher.UseTransport<RabbitMq>();
                publisher.MessagesAssemblies = new Assembly[] { Assembly.GetAssembly(typeof(RegisterAccount)), Assembly.GetAssembly(typeof(CreateUser)) };
            });
            cfg.ConfigureConsumer<EndpointProjectionConsumableSettings>(Collaboration, consumer =>
            {
                consumer.NumberOfWorkers = 2;
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
                consumer.UseTransport<RabbitMq>();
            })
            .Build();

            new CronusHost(cfg).Start();

            Console.WriteLine("Projections started");
            Console.ReadLine();
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