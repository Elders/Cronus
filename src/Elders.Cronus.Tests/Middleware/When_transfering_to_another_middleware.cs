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
    public class When_transfering_to_another_middleware
    {
        Establish context = () =>
        {
            expectedExecution = new List<ExecutionToken>();
            executionChain = new TestExecutionChain();
            var mainToken = executionChain.CreateToken();
            mainMiddleware = new TestMiddleware(mainToken);
            expectedExecution.Add(mainToken);


            var secondToken = executionChain.CreateToken();
            var secondMiddleware = new TestMiddleware(secondToken);
            mainMiddleware.Use((execution) =>
            {
                execution.Transfer(secondMiddleware);
            });
            expectedExecution.Add(secondToken);

            var thirdToken = executionChain.CreateToken();
            var thirdMiddleware = new TestMiddleware(thirdToken);
            mainMiddleware.Use(thirdMiddleware);

        };

        Because of = () => mainMiddleware.Run(invocationContext);

        It the_execution_chain_should_not_be_empty = () => executionChain.GetTokens().ShouldNotBeEmpty();

        It should_have_the_expected_execution = () => executionChain.ShouldMatch(expectedExecution);


        static TestMiddleware mainMiddleware;

        static TestExecutionChain executionChain;

        static List<ExecutionToken> expectedExecution;

        static string invocationContext = "Test context";

    }
}
