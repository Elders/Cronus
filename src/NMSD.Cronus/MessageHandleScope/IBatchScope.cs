namespace NMSD.Cronus.Messaging.MessageHandleScope
{
    public interface IBatchScope : IScope
    {
        //  TODO: Code contract >= 1
        int Size { get; set; }
        //OnError(Message[],Exception)
    }
}
