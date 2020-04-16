using Elders.Cronus.Workflow;
using Machine.Specifications;
using System.Collections.Generic;

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

            thirdToken = executionChain.CreateToken();
            var thirdMiddleware = new TestMiddleware(thirdToken);
            mainMiddleware.Use(thirdMiddleware);

        };

        Because of = () => mainMiddleware.Run(invocationContext);

        It the_execution_chain_should_not_be_empty = () => executionChain.GetTokens().ShouldNotBeEmpty();

        It should_have_the_expected_execution = () => executionChain.ShouldMatch(expectedExecution);

        It should_not_contain_the_third_token = () => executionChain.GetTokens().ShouldNotContain(thirdToken);


        static TestMiddleware mainMiddleware;

        static TestExecutionChain executionChain;

        static List<ExecutionToken> expectedExecution;

        static ExecutionToken thirdToken;

        static string invocationContext = "Test context";

    }

    [Subject("Elders.Cronus.Middleware")]
    public class When_overriding_middleware
    {
        Establish context = () =>
        {
            expectedExecution = new List<ExecutionToken>();
            executionChain = new TestExecutionChain();
            var mainToken = executionChain.CreateToken();
            mainMiddleware = new TestMiddleware(mainToken);
            expectedExecution.Add(mainToken);


            secondToken = executionChain.CreateToken();
            var secondMiddleware = new TestMiddleware(secondToken);
            mainMiddleware.Use((execution) =>
            {
                execution.Transfer(secondMiddleware);
            });

            var thirdToken = executionChain.CreateToken();
            var thirdMiddleware = new TestMiddleware(thirdToken);
            expectedExecution.Add(thirdToken);
            mainMiddleware.Override(thirdMiddleware);
        };

        Because of = () => mainMiddleware.Run(invocationContext);

        It the_execution_chain_should_not_be_empty = () => executionChain.GetTokens().ShouldNotBeEmpty();

        It should_have_the_expected_execution = () => executionChain.ShouldMatch(expectedExecution);

        It should_not_contain_the_second_token = () => executionChain.GetTokens().ShouldNotContain(secondToken);


        static TestMiddleware mainMiddleware;

        static TestExecutionChain executionChain;

        static List<ExecutionToken> expectedExecution;

        static ExecutionToken secondToken;

        static string invocationContext = "Test context";
    }
}
