namespace Elders.Cronus;

/// <summary>
/// This type of startups are singleton and are executed ONLY once, so use accordingly
/// </summary>
public interface ICronusStartup
{
    // TODO: Make this async
    void Bootstrap();
}

/// <summary>
/// This type of startups are executed X amount of times per tenant, so use accordingly
/// </summary>
public interface ICronusTenantStartup //TODO: also make this async :) kali
{
    void Bootstrap();
}
