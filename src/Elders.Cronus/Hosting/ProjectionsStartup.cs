using Elders.Cronus.Projections;

namespace Elders.Cronus;

[CronusStartup(Bootstraps.Projections)]
internal sealed class ProjectionsStartup : ICronusStartup
{
    private readonly CronusProjectionBootstrapper projectionsBootstrapper;

    public ProjectionsStartup(CronusProjectionBootstrapper projectionsBootstrapper)
    {
        this.projectionsBootstrapper = projectionsBootstrapper;
    }

    public void Bootstrap()
    {
        projectionsBootstrapper.BootstrapAsync().GetAwaiter().GetResult();
    }
}
