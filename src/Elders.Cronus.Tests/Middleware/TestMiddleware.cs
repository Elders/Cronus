using Elders.Cronus.Middleware;

namespace Elders.Cronus.Tests.Middleware
{
    public class TestMiddleware : Middleware<string>
    {
        ExecutionToken token;

        public TestMiddleware(ExecutionToken token)
        {
            this.token = token;
        }
        protected override void Run(Execution<string> context)
        {
            token.Notify();
        }
    }
}