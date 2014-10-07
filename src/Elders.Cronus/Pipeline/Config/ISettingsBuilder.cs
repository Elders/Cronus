using Elders.Cronus.IocContainer;

namespace Elders.Cronus.Pipeline.Config
{
    public interface ISettingsBuilder
    {
        IContainer Container { get; set; }
        string Name { get; set; }
        void Build();
    }
}