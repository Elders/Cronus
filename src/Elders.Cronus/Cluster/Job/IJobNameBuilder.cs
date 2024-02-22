namespace Elders.Cronus.Cluster.Job;

public interface IJobNameBuilder
{
    string GetJobName(string defaultName);
}
