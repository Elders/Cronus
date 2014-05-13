using System.Reflection;
using Elders.Cronus.Pipeline.Hosts;

namespace Elders.Cronus.Pipeline.Config
{
    public abstract class PublisherSettings : IPublisherSettings
    {
        public CronusGlobalSettings GlobalSettings { get; set; }

        public Assembly[] MessagesAssemblies { get; set; }
    }
}
