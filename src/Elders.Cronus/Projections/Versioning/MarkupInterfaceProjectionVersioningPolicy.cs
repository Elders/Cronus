using System;

namespace Elders.Cronus.Projections.Versioning;

public class MarkupInterfaceProjectionVersioningPolicy : IProjectionVersioningPolicy
{
    public bool IsVersionable(string projectionName)
    {
        try
        {
            return typeof(INonVersionableProjection).IsAssignableFrom(MessageInfo.GetTypeByContract(projectionName)) == false;
        }
        catch (Exception)
        {
            return true;
        }
    }
}
