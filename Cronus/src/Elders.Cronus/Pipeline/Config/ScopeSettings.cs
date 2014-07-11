using Elders.Cronus.UnitOfWork;

namespace Elders.Cronus.Pipeline.Config
{
    public class UnitOfWorkSettings : HideObectMembers
    {
        public IBatchUnitOfWork BatchUnitOfWork { get; set; }
        public IMessageUnitOfWork MessageUnitOfWork { get; set; }
        public IHandlerUnitOfWork HandlerUnitOfWork { get; set; }
    }
}