using Machine.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Tests.Middleware;

public class TestExecutionChain
{
    Queue<ExecutionToken> executionChain;

    public TestExecutionChain()
    {
        executionChain = new Queue<ExecutionToken>();
    }

    public void AddToken(ExecutionToken token)
    {
        executionChain.Enqueue(token);
    }

    public ExecutionToken CreateToken(string name = null)
    {
        if (name == null)
            name = Guid.NewGuid().ToString();

        return new ExecutionToken(name, this);
    }

    public List<ExecutionToken> GetTokens()
    {
        return executionChain.ToList();
    }

    public void ShouldMatch(List<ExecutionToken> chain)
    {
        var actualExecution = this.GetTokens();
        actualExecution.Count.ShouldEqual(chain.Count);
        for (int i = 0; i < actualExecution.Count; i++)
        {
            actualExecution[i].ShouldEqual(chain[i]);
        }
    }
}