namespace Elders.Cronus.UnitOfWork
{
    public interface IUnitOfWork
    {
        void Begin();

        void End();

        IUnitOfWorkContext Context { get; set; }
    }
}