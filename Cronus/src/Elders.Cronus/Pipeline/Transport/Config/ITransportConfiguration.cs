namespace Elders.Cronus.Pipeline.Transport.Config
{
    public interface ITransportConfiguration<TTransportSettings>
        where TTransportSettings : ITransportSettings
    {
        TTransportSettings TransportSettings { get; set; }
    }
}