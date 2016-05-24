using Elders.Cronus.Middleware;
using Machine.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elders.Cronus.Tests.Middleware
{
    [Subject("Elders.Cronus.Middleware")]
    public class When_executing_single_middleware
    {
        Establish context = () =>
        {
            executionChain = new TestExecutionChain();
            firstToken = executionChain.CreateToken("First token");
            mainMiddleware = new TestMiddleware(firstToken);
        };

        Because of = () => mainMiddleware.Invoke(invocationContext);

        It the_execution_chain_should_not_be_empty = () => executionChain.GetTokens().ShouldNotBeEmpty();

        It should_have_executed_only_once = () => executionChain.GetTokens().SingleOrDefault().ShouldNotBeNull();

        It should_have_notified_the_first_token = () => executionChain.GetTokens().SingleOrDefault().ShouldEqual(firstToken);


        static TestMiddleware mainMiddleware;

        static TestExecutionChain executionChain;

        static ExecutionToken firstToken;

        static string invocationContext = "Test context";

    }
}
