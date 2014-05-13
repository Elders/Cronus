using System.Reflection;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IPublisherSettings
    {
        Assembly[] MessagesAssemblies { get; set; }
    }

    public static class PublisherSettingsExtensions
    {

    }
}