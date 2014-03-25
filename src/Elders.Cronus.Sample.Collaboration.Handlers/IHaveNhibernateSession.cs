using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;

namespace Elders.Cronus.Sample.Collaboration
{
    public interface IHaveNhibernateSession
    {
        ISession Session { get; set; }
    }
}
