using System;
using System.Collections.Generic;

namespace Elders.Cronus.UnitOfWork
{
    public class UnitOfWorkContext : IUnitOfWorkContext
    {
        Dictionary<string, Dictionary<Type, object>> context = new Dictionary<string, Dictionary<Type, object>>();

        public UnitOfWorkContext()
        {
            context = new Dictionary<string, Dictionary<Type, object>>();
        }

        public void Set<T>(T obj)
        {
            var objType = obj.GetType();
            var objName = objType.FullName;
            Set<T>(objName, objType, obj);
        }

        public void Set<T>(string name, T obj)
        {
            var objType = obj.GetType();
            Set<T>(name, objType, obj);
        }

        private void Set<T>(string name, Type objType, object obj)
        {
            if (!context.ContainsKey(name))
                context.Add(name, new Dictionary<Type, object>());

            context[name].Add(objType, obj);
        }

        public T Get<T>()
        {
            var objName = typeof(T).FullName;
            return Get<T>(objName);
        }

        public T Get<T>(string name)
        {
            object obj;
            Dictionary<Type, object> objects;

            var objType = typeof(T);

            if (context.TryGetValue(name, out objects) && objects.TryGetValue(objType, out obj))
            {
                return (T)obj;
            }
            else
                return default(T);
        }

        public void Clear()
        {
            if (context != null)
            {
                context.Clear();
                context = null;
            }
        }
    }
}