using System;
using System.Collections.Generic;

namespace Elders.Cronus.MessageProcessing
{
    public interface ISubscriberFinder<T>
    {
        IEnumerable<Type> Find();
    }
}
