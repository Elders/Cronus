using Elders.Cronus.IocContainer;

namespace Elders.Cronus.Pipeline.Config
{
    public interface ISettingsBuilder
    {
        IContainer Container { get; set; }
        string Name { get; set; }
        void Build();
    }

    public abstract class SettingsBuilder : ISettingsBuilder
    {
        public SettingsBuilder(ISettingsBuilder settingsBuilder, string name = null)
        {
            (this as ISettingsBuilder).Container = settingsBuilder.Container;
            (this as ISettingsBuilder).Name = name ?? settingsBuilder.Name;
        }

        IContainer ISettingsBuilder.Container { get; set; }

        string ISettingsBuilder.Name { get; set; }

        public abstract void Build();
    }
}