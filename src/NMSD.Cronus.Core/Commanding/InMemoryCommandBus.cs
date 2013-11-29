using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.Core.Publishing;

namespace NMSD.Cronus.Core.Commanding
{
    public class InMemoryCommandBus : InMemoryPublisher<ICommand, ICommandHandler>
    {

    }
}