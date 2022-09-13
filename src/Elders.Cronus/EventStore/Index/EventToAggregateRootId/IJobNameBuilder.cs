namespace Elders.Cronus.EventStore.Index
{
    public interface IJobNameBuilder
    {
        string GetJobName(string defaultName);
    }
}
