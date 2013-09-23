using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cronus.Core.Eventing;

namespace Cronus.Sample.ConsoleApplication
{
    public class TestProjections : IEventHandler,
        IEventHandler<TestEvent>
    {
        public void Handle(TestEvent evnt)
        {
            throw new Exception("Error1");
        }
    }

    public class TestProjections1 : IEventHandler,
        IEventHandler<TestEvent>
    {
        public void Handle(TestEvent evnt)
        {
            throw new Exception("Error2");
        }
    }

    public class TestProjections2 : IEventHandler,
        IEventHandler<TestEvent>
    {
        public void Handle(TestEvent evnt)
        {
            for (int i = 0; i < 15; i++)
            {
                Thread.Sleep(300);
                Console.WriteLine(2);
            }
        }
    }

    public class TestProjections3 : IEventHandler,
        IEventHandler<TestEvent>
    {
        public void Handle(TestEvent evnt)
        {
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(400);
                Console.WriteLine(1);
            }
        }
    }
}
