using System;

namespace Elders.Cronus
{
    public static class ObjectExtensions
    {
        public static object AssignPropertySafely<TContract>(this object self, Action<TContract> assignProperty)
        {
            var canProceed = typeof(TContract).IsAssignableFrom(self.GetType());
            if (canProceed)
            {
                var contract = (TContract)self;
                assignProperty(contract);
                self = contract;
            }
            return self;
        }
    }
}