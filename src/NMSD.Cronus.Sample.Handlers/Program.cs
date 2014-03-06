using System;
using System.Linq;
using System.Reflection;
using NHibernate;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Pipelining;
using NMSD.Cronus.Pipelining.RabbitMQ.Config;
using NMSD.Cronus.Pipelining.Transport.Config;
using NMSD.Cronus.Sample.Collaboration;
using NMSD.Cronus.Sample.Collaboration.Users.Commands;
using NMSD.Cronus.Sample.Collaboration.Projections;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts.Commands;
using NMSD.Cronus.Sample.InMemoryServer.Nhibernate;
using NMSD.Cronus.Sample.Player;
using NMSD.Cronus.Pipelining.Host.Config;

namespace NMSD.Cronus.Sample.Handlers
{
    class Program
    {

        public static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            //var sf = BuildSessionFactory();
            var cfg = new CronusConfiguration();

            string Collaboration = "Collaboration";
            cfg.ConfigurePublisher<PipelinePublisher<ICommand>>(Collaboration, publisher =>
            {
                publisher.RabbitMq();
                publisher.MessagesAssemblies = new Assembly[] { Assembly.GetAssembly(typeof(RegisterAccount)), Assembly.GetAssembly(typeof(CreateUser)) };
            });
            cfg.ConfigureConsumer<EndpointConsumer<IEvent>>(Collaboration, consumer =>
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
                            port.CommandPublisher = cfg.publishers[Collaboration][typeof(ICommand)];
                        }
                        return handler;
                    });
                consumer.RabbitMq();
            });
            cfg.Start();

            Console.WriteLine("Started");
            Console.ReadLine();
        }

        static ISessionFactory BuildSessionFactory()
        {
            var typesThatShouldBeMapped = Assembly.GetAssembly(typeof(UserProjection)).GetExportedTypes().Where(t => t.Namespace.EndsWith("DTOs"));
            var cfg = new NHibernate.Cfg.Configuration();
            cfg = cfg.AddAutoMappings(typesThatShouldBeMapped);
            cfg.CreateDatabaseTables();
            return cfg.BuildSessionFactory();
        }
    }
}