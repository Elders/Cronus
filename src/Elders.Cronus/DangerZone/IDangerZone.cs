using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elders.Cronus.DangerZone;

public interface IDangerZone
{
    Task WipeDataAsync(string tenant);
}
