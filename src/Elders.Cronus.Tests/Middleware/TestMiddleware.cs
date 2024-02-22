using Elders.Cronus.Workflow;
using System.Threading.Tasks;

namespace Elders.Cronus.Tests.Middleware;

public class TestMiddleware : Workflow<string>
{
    ExecutionToken token;

    public TestMiddleware(ExecutionToken token)
    {
        this.token = token;
    }

    protected override Task RunAsync(Execution<string> execution)
    {
        token.Notify();
        return Task.CompletedTask;
    }
}
