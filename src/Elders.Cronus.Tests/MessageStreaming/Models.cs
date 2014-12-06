using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Tests.MessageStreaming
{
    public class HandleResult
    {
        public static int Total = 0;
    }

    public class TestAddHandler1
    {
        public void Handle(TestIntMessage1 message)
        {
            HandleResult.Total += message.Value;
        }
    }

    public class TestAddHandler2
    {
        public void Handle(TestIntMessage1 message)
        {
            HandleResult.Total += message.Value;
        }
    }

    public class TestSubstractHandler1
    {
        public void Handle(TestIntMessage2 message)
        {
            HandleResult.Total -= message.Value;
        }
    }

    public class TestIntMessage1 : IMessage
    {
        public TestIntMessage1(int value)
        {
            Value = value;
        }

        public int Value { get; set; }
    }

    public class TestIntMessage2 : IMessage
    {
        public TestIntMessage2(int value)
        {
            Value = value;
        }

        public int Value { get; set; }
    }
}