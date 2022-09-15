using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus
{
    public class TypeContainer<T>
    {
        public TypeContainer() { }
        public TypeContainer(IEnumerable<Type> items)
        {
            var expectedType = typeof(T);
            Items = items.Where(type => expectedType.IsAssignableFrom(type));
        }
        public IEnumerable<Type> Items { get; set; }
    }
}
