namespace Elders.Cronus.AutoUpdates;

public interface IAutoUpdaterStrategy
{
    IAutoUpdate GetInstanceFor(string name);
}
