namespace Elders.Cronus;

public interface ISerializer
{
    byte[] SerializeToBytes<T>(T message);
    string SerializeToString<T>(T message);
    T DeserializeFromBytes<T>(byte[] bytes);
}
