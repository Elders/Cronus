using System;
using System.Collections.Generic;
using NMSD.Cronus.Multithreading.Work;
using NMSD.Cronus.Pipelining;
using NMSD.Cronus.Transports;
using NMSD.Cronus.Transports.RabbitMQ;

namespace NMSD.Cronus.Messaging.MessageHandleScope
{
    public class ScopeContext : IScopeContext
    {
        Dictionary<Type, object> context = new Dictionary<Type, object>();
        public ScopeContext()
        {

        }

        public void Set<T>(T obj)
        {
            context.Add(typeof(T), obj);
        }

        public T Get<T>()
        {
            object obj;
            if (context.TryGetValue(typeof(T), out obj))
            {
                return (T)obj;
            }
            else
                return default(T);

        }

        public void Clear()
        {
            context.Clear();
            context = null;
        }
    }
}