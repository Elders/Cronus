using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMSD.Cronus.Core.UnitOfWork
{
    public interface IUnitOfWorkPerHandler : IUnitOfWork
    {
        IUnitOfWorkPerMessage UoWMessage { get; set; }
    }
}
