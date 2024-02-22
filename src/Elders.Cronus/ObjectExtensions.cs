using System;

namespace Elders.Cronus;

public static class ObjectExtensions
{
    public static object AssignPropertySafely<TContract>(this object self, Action<TContract> assignProperty)
    {
        if (self is TContract casted)
        {
            assignProperty(casted);
            self = casted;
        }

        return self;
    }
}
