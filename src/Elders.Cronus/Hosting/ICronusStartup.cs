using System.Threading;
using System.Threading.Tasks;

namespace Elders.Cronus;

/// <summary>
/// This type of startups are singleton and are executed ONLY once, so use accordingly
/// </summary>
public interface ICronusStartup
{
    Task BootstrapAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// This type of startups are executed X amount of times per tenant, so use accordingly
/// </summary>
public interface ICronusTenantStartup
{
    Task BootstrapAsync(CancellationToken cancellationToken = default);
}
