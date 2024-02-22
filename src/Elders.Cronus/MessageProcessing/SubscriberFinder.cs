using System;
using System.Collections.Generic;

namespace Elders.Cronus.MessageProcessing;

public class SubscriberFinder<T> : ISubscriberFinder<T>
{
    private readonly TypeContainer<T> typeContainer;

    public SubscriberFinder(TypeContainer<T> typeContainer)
    {
        this.typeContainer = typeContainer;
    }

    public IEnumerable<Type> Find()
    {
        return typeContainer.Items;
    }
}
