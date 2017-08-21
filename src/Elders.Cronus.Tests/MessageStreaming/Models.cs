using System;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Tests.MessageStreaming
{
    public class CalculatorState
    {
        public int Total = 0;
    }

    public class CalculatorHandlerFactory : IHandlerFactory
    {
        public CalculatorState State { get; private set; }
        public CalculatorHandlerFactory() { State = new CalculatorState(); }

        public IHandlerInstance Create(Type handlerType)
        {
            object instance = FastActivator.CreateInstance(handlerType);
            ((dynamic)instance).State = (dynamic)State;
            return new DefaultHandlerInstance(instance);
        }
    }

    public class StandardCalculatorAddHandler
    {
        public CalculatorState State { get; set; }
        public void Handle(CalculatorNumber1 message) { State.Total += message.Value; }
        public void Handle(CalculatorNumber2 message) { State.Total += message.Value; }
    }

    public class ScientificCalculatorHandler
    {
        public CalculatorState State { get; set; }
        public void Handle(CalculatorNumber1 message) { State.Total += message.Value; }
    }

    public class StandardCalculatorSubstractHandler
    {
        public CalculatorState State { get; set; }
        public void Handle(CalculatorNumber2 message) { State.Total -= message.Value; }
    }

    public class CalculatorHandler_ThrowsException
    {
        public CalculatorState State { get; set; }
        public void Handle(CalculatorNumber1 message) { throw new Exception(); }
        public void Handle(CalculatorNumber2 message) { throw new Exception(); }
    }

    public class CalculatorNumber1 : IMessage
    {
        public CalculatorNumber1(int value) { Value = value; }
        public int Value { get; set; }
    }

    public class CalculatorNumber2 : IMessage
    {
        public CalculatorNumber2(int value) { Value = value; }
        public int Value { get; set; }
    }
}
