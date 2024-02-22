namespace Elders.Cronus.Projections.Versioning;

public interface IProjectionVersioningPolicy
{
    bool IsVersionable(string projectionName);
}
