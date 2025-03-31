using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elders.Cronus.DangerZone;

public sealed class DangerZoneExecutor
{
    private readonly IEnumerable<IDangerZone> dangerZones;

    public DangerZoneExecutor(IEnumerable<IDangerZone> dangerZones)
    {
        this.dangerZones = dangerZones;
    }

    public async Task WipeDataAsync(string tenant)
    {
        //TODO: Add Security checks

        foreach (var item in dangerZones)
        {
            await item.WipeDataAsync(tenant).ConfigureAwait(false);
        }
    }
}
