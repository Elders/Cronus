namespace Elders.Cronus.Tests.IocContainer
{
    public class BatchComponent
    {
        public BatchComponent() { }

        public BatchComponent(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    public interface ITestUntOfWork
    {
        string Name { get; }
    }

    public class PerMessageUnitOfWork : ITestUntOfWork
    {
        public string Name { get { return "PerMessageUnitOfWork"; } }
    }

    public class PerHandlerUnitOfWork : ITestUntOfWork
    {
        public string Name { get { return "PerHandlerUnitOfWork"; } }
    }
}