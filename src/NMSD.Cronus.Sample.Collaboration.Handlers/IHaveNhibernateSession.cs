using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;

namespace NMSD.Cronus.Sample.Nhibernate.UoW
{
    public interface IHaveNhibernateSession
    {
        ISession Session { get; set; }
    }
}
