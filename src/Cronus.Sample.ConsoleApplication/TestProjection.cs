using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cronus.Core.Eventing;

namespace Cronus.Sample.ConsoleApplication
{
    public class TestProjection : IEventHandler,
        IEventHandler<TestEvent>
    {
        public void Handle(TestEvent evnt)
        {
            int i = 0;
            i++;
        }
    }
}
