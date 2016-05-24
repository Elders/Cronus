using Elders.Cronus.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elders.Cronus.Tests.Middleware
{
    public class TestMiddleware : Middleware<string>
    {
        ExecutionToken token;

        public TestMiddleware(ExecutionToken token)
        {
            this.token = token;
        }
        protected override void Invoke(MiddlewareContext<string> context)
        {
            token.Notify();
        }
    }
}