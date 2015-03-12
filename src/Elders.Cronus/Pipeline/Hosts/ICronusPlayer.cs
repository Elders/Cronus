using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elders.Cronus.Pipeline.Hosts
{
    public interface ICronusPlayer : IDisposable
    {
        bool Replay();
        bool Stop();
    }
}
