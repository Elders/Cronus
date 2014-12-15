using System;

namespace Elders.Cronus.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IDisposable Begin();

        Guid Id { get; }

        IUnitOfWorkContext Context { get; set; }
    }

    public class NoUnitOfWork : IUnitOfWork
    {
        public IDisposable Begin() { return this; }

        public void Dispose() { }

        public IUnitOfWorkContext Context { get; set; }

        public Guid Id { get { return Guid.Empty; } }
    }
}