using System;

namespace Elders.Cronus.UnitOfWork
{
    public class Context
    {
        public IUnitOfWorkContext BatchContext { get; set; }
        public IUnitOfWorkContext MessageContext { get; set; }
        public IUnitOfWorkContext HandlerContext { get; set; }
    }
}