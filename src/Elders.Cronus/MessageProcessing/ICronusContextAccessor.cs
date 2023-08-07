namespace Elders.Cronus.MessageProcessing
{
    public interface ICronusContextAccessor
    {
        CronusContext CronusContext { get; set; }
    }
}
