using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Middleware
{
    public static class LinkedListExtensions
    {
        public static T Dequeue<T>(this LinkedList<T> self)
        {
            var last = self.Last;
            self.RemoveLast();
            return last.Value;
        }

        public static void Enqueue<T>(this LinkedList<T> self, T value)
        {
            self.AddFirst(value);
        }

        public static void Push<T>(this LinkedList<T> self, T value)
        {
            self.AddLast(value);
        }

        public static void PushMany<T>(this LinkedList<T> self, IEnumerable<T> value)
        {
            foreach (var item in value.Reverse())
            {
                self.Push(item);
            }
        }

        public static T Pop<T>(this LinkedList<T> self)
        {
            return Dequeue(self);
        }
    }
}
