using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.EventSourcing;

namespace NMSD.Cronus.Player
{
    public interface IEventPlayer
    {
        void ReplayEvents();
    }
}
