using System.Threading.Tasks;

namespace Elders.Cronus.AutoUpdates;

/// <summary>
/// Be aware this is executed per tenant and auto updates are tracked in <see cref="AutoUpdater"/>
/// </summary>
public interface IAutoUpdate
{
    public uint ExecutionSequence { get; }

    public string Name { get; }

    Task<bool> ApplyAsync();
}
