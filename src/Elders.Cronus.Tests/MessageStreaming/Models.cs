using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.UnitOfWork;

namespace Elders.Cronus.Tests.MessageStreaming
{
    public class CalculatorState
    {
        public int Total = 0;
    }

    public class CalculatorHandlerFactory
    {
        public CalculatorState State { get; private set; }
        public CalculatorHandlerFactory() { State = new CalculatorState(); }
        public object CreateInstance(Type t)
        {
            object instance = FastActivator.CreateInstance(t);
            ((dynamic)instance).State = (dynamic)State;
            return instance;
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

    public class TestPerBatchUnitOfWork_ThrowsExceptoin : IUnitOfWork
    {
        public IUnitOfWorkContext Context { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
        public string Id { get { return Guid.NewGuid().ToString(); } }

        public IDisposable Begin() { return this; }
        public void Dispose() { throw new NotImplementedException(); }
    }
}