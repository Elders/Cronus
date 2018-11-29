using Elders.Cronus.Workflow;

namespace Elders.Cronus.Tests.Middleware
{
    public class TestMiddleware : Workflow<string>
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