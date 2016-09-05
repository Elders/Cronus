using Machine.Specifications;
using System.Collections.Generic;

namespace Elders.Cronus.Tests.Middleware
{
    [Subject("Elders.Cronus.Middleware")]
    public class When_executing_middleware_tree
    {
        Establish context = () =>
        {
            executionChain = new TestExecutionChain();
            expectedExecution = new List<ExecutionToken>();
            var firstToken = executionChain.CreateToken();
            mainMiddleware = new TestMiddleware(firstToken);
            expectedExecution.Add(firstToken);


            var secondLeaf = executionChain.CreateToken();
            var secondMiddleware = new TestMiddleware(secondLeaf);
            expectedExecution.Add(secondLeaf);
            mainMiddleware.Use(secondMiddleware);


            var thirdLeaf = executionChain.CreateToken();
            var thirdMiddleware = new TestMiddleware(thirdLeaf);
            expectedExecution.Add(thirdLeaf);
            secondMiddleware.Use(thirdMiddleware);

            var forthLeaf = executionChain.CreateToken();
            var forthMiddleware = new TestMiddleware(forthLeaf);
            expectedExecution.Add(forthLeaf);
            mainMiddleware.Use(forthMiddleware);


        };

        Because of = () => mainMiddleware.Run(invocationContext);

        It the_execution_chain_should_not_be_empty = () => executionChain.GetTokens().ShouldNotBeEmpty();

        It should_have_multiple_execution_tokens = () => executionChain.GetTokens().Count.ShouldEqual(expectedExecution.Count);

        It should_have_the_expected_execution = () => executionChain.ShouldMatch(expectedExecution);


        static TestMiddleware mainMiddleware;

        static TestExecutionChain executionChain;

        static List<ExecutionToken> expectedExecution;

        static string invocationContext = "Test context";

    }
}
