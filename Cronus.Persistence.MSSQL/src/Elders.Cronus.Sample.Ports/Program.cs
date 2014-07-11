using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.UnitOfWork;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Transport.RabbitMQ.Config;
using Elders.Cronus.Sample.Collaboration;
using Elders.Cronus.Sample.Collaboration.Projections;
using Elders.Cronus.Sample.Collaboration.Users.Commands;
using Elders.Cronus.Sample.Collaboration.Users.DTOs;
using Elders.Cronus.Sample.CommonFiles;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Commands;
using Elders.Cronus.Sample.Ports.Nhibernate;
using NHibernate;
using NHibernate.Mapping.ByCode;

namespace Elders.Cronus.Sample.Ports
{
    class Program
    {
        static CronusHost host;
        public static void Main(string[] args)
        {
            Thread.Sleep(9000);
            log4net.Config.XmlConfigurator.Configure();

            var sf = BuildSessionFactory();
            var cfg = new CronusSettings()
                .UseContractsFromAssemblies(new Assembly[] { Assembly.GetAssembly(typeof(RegisterAccount)), Assembly.GetAssembly(typeof(CreateUser)) })
                .WithDefaultPublishersWithRabbitMq();

            const string Collaboration = "Collaboration";
            cfg
                .UsePipelineCommandPublisher(publisher => publisher.UseRabbitMqTransport())
                .UsePortConsumable(Collaboration, consumable => consumable
                    .SetNumberOfConsumers(2)
                    .UseRabbitMqTransport()
                    .EventConsumer(consumer => consumer
                        .UsePortHandler(h => h
                            .UseUnitOfWork(new UnitOfWorkFactory() { CreateBatchUnitOfWork = () => new BatchUnitOfWork(sf) })
                            .RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserProjection)), (type, context) =>
                            {
                                return FastActivator.CreateInstance(type)
                                    .AssignPropertySafely<IHaveNhibernateSession>(x => x.Session = context.BatchContext.Get<Lazy<ISession>>().Value)
                                    .AssignPropertySafely<IPort>(x => x.CommandPublisher = (cfg as IHaveCommandPublisher).CommandPublisher.Value);
                            }))));

            host = new CronusHost(cfg.GetInstance());
            host.Start();

            Console.WriteLine("Ports started");
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
            return cfg.BuildSessionFactory();
        }
    }
}