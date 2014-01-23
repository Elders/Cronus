using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using NMSD.Cronus.Eventing;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Transports.Conventions;
using NMSD.Cronus.Transports.RabbitMQ;
using NMSD.Cronus.UnitOfWork;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
using NMSD.Cronus.Sample.IdentityAndAccess.Users;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Events;
using NMSD.Protoreg;
using NMSD.Cronus.Hosts;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands;

namespace NMSD.Cronus.Sample.EventStore
{
    class Program
    {
        private static CronusHost host;
        static void Main(string[] args)
        {
            //log4net.Config.XmlConfigurator.Configure();

            //var protoRegistration = new ProtoRegistration();
            //protoRegistration.RegisterAssembly<CollaboratorState>();
            //protoRegistration.RegisterAssembly<NewCollaboratorCreated>();
            //protoRegistration.RegisterAssembly<UserState>();
            //protoRegistration.RegisterAssembly<NewUserRegistered>();
            //protoRegistration.RegisterAssembly<Wraper>();
            //ProtoregSerializer serializer = new ProtoregSerializer(protoRegistration);
            //serializer.Build();
            //var rabbitMqSessionFactory = new RabbitMqSessionFactory();
            //var session = rabbitMqSessionFactory.OpenSession();
            //string connectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString;
            //var eventPublisher = new EventPublisher(new EventHandlersPipelinePerApplication(), new RabbitMqPipelineFactory(session), serializer);
            //var iacES = new MssqlEventStore("IdentityAndAccess", connectionString, serializer);
            //var iacEventStoreConsumer = new EventStoreConsumer(new EventStorePerBoundedContext(new EventStorePipelinePerApplication()), new RabbitMqEndpointFactory(session), Assembly.GetAssembly(typeof(NewUserRegistered)), serializer, iacES, eventPublisher);
            //iacEventStoreConsumer.UnitOfWorkFactory = new NullUnitOfWorkFactory();
            //iacEventStoreConsumer.Start(5);

            //var collaborationES = new MssqlEventStore("Collaboration", connectionString, serializer);
            //var collaborationEventStoreConsumer = new EventStoreConsumer(new EventStorePerBoundedContext(new EventStorePipelinePerApplication()), new RabbitMqEndpointFactory(session), Assembly.GetAssembly(typeof(NewCollaboratorCreated)), serializer, collaborationES, eventPublisher);
            //collaborationEventStoreConsumer.UnitOfWorkFactory = new NullUnitOfWorkFactory();
            //collaborationEventStoreConsumer.Start(5);
            UseCronusHost();
            System.Console.WriteLine("Started");
            System.Console.ReadLine();
            host.Release();
            //  session.Close();


        }
        static void UseCronusHost()
        {
            host = new CronusHost();
            host.UseRabbitMqTransport();
            host.UseEventHandlersPipelinePerApplication();
            host.UseEventStorePipelinePerApplication();
            host.UseEventStorePerBoundedContext();
            host.ConfigureEventStoreConsumer(cfg =>
            {
                cfg.SetEventStoreConnectionString(ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString);
                cfg.SetUnitOfWorkFacotry(new NullUnitOfWorkFactory());
                cfg.SetAggregateStatesAssembly(Assembly.GetAssembly(typeof(CollaboratorState)));
                cfg.SetEventsAssembly(Assembly.GetAssembly(typeof(NewCollaboratorCreated)));
            });
            host.ConfigureEventStoreConsumer(cfg =>
            {
                cfg.SetEventStoreConnectionString(ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString);
                cfg.SetUnitOfWorkFacotry(new NullUnitOfWorkFactory());
                cfg.SetAggregateStatesAssembly(Assembly.GetAssembly(typeof(UserState)));
                cfg.SetEventsAssembly(Assembly.GetAssembly(typeof(NewUserRegistered)));
            });
            host.BuildSerializer();
            host.HostEventStoreConsumers(1);
        }
    }
}
