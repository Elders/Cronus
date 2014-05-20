using Elders.Protoreg;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IHaveSerializer
    {
        ProtoregSerializer Serializer { get; set; }
    }

    public interface ICanConfigureSerializer : IHaveSerializer { }
}