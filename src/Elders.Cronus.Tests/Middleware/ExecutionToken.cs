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