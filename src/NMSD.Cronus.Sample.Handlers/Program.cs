using System;
using System.Linq;
using System.Reflection;
using NHibernate;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Pipeline.Config;
using NMSD.Cronus.Pipeline.Hosts;
using NMSD.Cronus.Pipeline.Transport.RabbitMQ.Config;
using NMSD.Cronus.Sample.Collaboration.Projections;
using NMSD.Cronus.Sample.Collaboration.Users.Commands;
using NMSD.Cronus.Sample.CommonFiles;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts.Commands;

namespace NMSD.Cronus.Sample.Handlers
{
    class Program
    {

        public static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            //var sf = BuildSessionFactory();
            var cfg = new CronusConfiguration();

            const string Collaboration = "Collaboration";
            cfg.PipelineCommandPublisher(publisher =>
            {
                publisher.UseTransport<RabbitMq>();
                publisher.MessagesAssemblies = new Assembly[] { Assembly.GetAssembly(typeof(RegisterAccount)), Assembly.GetAssembly(typeof(CreateUser)) };
            });
            cfg.ConfigureConsumer<EndpointEventConsumableSettings>(Collaboration, consumer =>
            {
                //consumer.ScopeFactory.CreateHandlerScope = () => new NHibernateHandlerScope(sf);
                consumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserProjection)), (type, context) =>
                    {
                        var handler = FastActivator.CreateInstance(type, null);
                        //var nhHandler = handler as IHaveNhibernateSession;
                        //if (nhHandler != null)
                        //{
                        //    nhHandler.Session = context.HandlerScopeContext.Get<ISession>();
                        //}
                        var port = handler as IPort;
                        if (port != null)
                        {
                            port.CommandPublisher = cfg.GlobalSettings.CommandPublisher;
                        }
                        return handler;
                    });
                consumer.UseTransport<RabbitMq>();
            })
            .Build();

            new CronusHost(cfg).Start();

            Console.WriteLine("Started");
            Console.ReadLine();
        }

        static ISessionFactory BuildSessionFactory()
        {
            var typesThatShouldBeMapped = Assembly.GetAssembly(typeof(UserProjection)).GetExportedTypes().Where(t => t.Namespace.EndsWith("DTOs"));
            var cfg = new NHibernate.Cfg.Configuration();
            cfg = cfg.AddAutoMappings(typesThatShouldBeMapped);
            cfg.CreateTables();
            return cfg.BuildSessionFactory();
        }
    }
}