using Elders.Cronus.Serializer;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IHaveSerializer
    {
        ISerializer Serializer { get; set; }
    }

    public interface ICanConfigureSerializer : IHaveSerializer { }
}