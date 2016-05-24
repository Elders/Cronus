using Elders.Cronus.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elders.Cronus.Tests.Middleware
{
    public class ExecutionToken
    {
        TestExecutionChain chain;

        public string Name { get; private set; }

        public ExecutionToken(string name, TestExecutionChain chain)
        {
            Name = name;
            this.chain = chain;
        }

        public void Notify()
        {
            chain.AddToken(this);
        }
    }
}