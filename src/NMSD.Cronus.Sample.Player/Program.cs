using System.Configuration;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Player;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands;
using NMSD.Protoreg;

namespace NMSD.Cronus.Sample.Player
{
    class Program
    {
        static void Main(string[] args)
        {
            //log4net.Config.XmlConfigurator.Configure();

            var protoRegistration = new ProtoRegistration();
            protoRegistration.RegisterAssembly<RegisterNewUser>();
            protoRegistration.RegisterAssembly<Wraper>();
            ProtoregSerializer serializer = new ProtoregSerializer(protoRegistration);
            serializer.Build();

            string connectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString;
            var eventStore = new MssqlEventStore("IdentityAndAccess", connectionString, serializer);
            var player = new EventPlayer(eventStore);
            player.ReplayEvents();
        }
    }
}
