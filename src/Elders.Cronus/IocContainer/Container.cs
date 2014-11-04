using System;
using System.Collections.Generic;

namespace Elders.Cronus.IocContainer
{
    /// <summary>
    /// IoC container
    /// </summary>
    public class Container : IContainer
    {
        /// <summary>
        /// Key: object containing the type of the object to resolve and the name of the instance (if any);
        /// Value: delegate that creates the instance of the object mapped
        /// </summary>
        private readonly Dictionary<MappingKey, Func<object>> transientMappings;
        private readonly Dictionary<MappingKey, Lazy<object>> singletonMappings;
        private readonly Dictionary<Type, List<MappingKey>> mappings;

        /// <summary>
        /// Creates a new instance of <see cref="Container"/>
        /// </summary>
        public Container()
        {
            this.transientMappings = new Dictionary<MappingKey, Func<object>>();
            this.singletonMappings = new Dictionary<MappingKey, Lazy<object>>();
            this.mappings = new Dictionary<Type, List<MappingKey>>();
        }

        /// <summary>
        /// Register a type with transient life style
        /// </summary>
        /// <param name="type">Type that will be requested</param>
        /// <param name="createInstanceDelegate">A delegate that will be used to 
        /// create an instance of the requested object</param>
        /// <param name="instanceName">Instance name (optional)</param>
        public void RegisterTransient(Type type, Func<object> createInstanceDelegate, string instanceName = null)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (createInstanceDelegate == null) throw new ArgumentNullException("createInstanceDelegate");

            var key = new MappingKey(type, instanceName);

            if (transientMappings.ContainsKey(key) || transientMappings.ContainsKey(key))
            {
                const string errorMessageFormat = "The requested mapping already exists - {0}";
                throw new InvalidOperationException(string.Format(errorMessageFormat, key.ToTraceString()));
            }

            transientMappings.Add(key, createInstanceDelegate);
            AddMapping(type, key);
        }

        private void AddMapping(Type type, MappingKey key)
        {
            List<MappingKey> mapsForCurrentType; ;
            if (mappings.TryGetValue(type, out mapsForCurrentType))
                mapsForCurrentType.Add(key);
            else
                mappings.Add(type, new List<MappingKey>() { key });
        }

        /// <summary>
        /// Register a type with singleton life style
        /// </summary>
        /// <param name="type">Type that will be requested</param>
        /// <param name="createInstanceDelegate">A delegate that will be used to 
        /// create an instance of the requested object</param>
        /// <param name="instanceName">Instance name (optional)</param>
        public void RegisterSingleton(Type type, Func<object> createInstanceDelegate, string instanceName = null)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (createInstanceDelegate == null) throw new ArgumentNullException("createInstanceDelegate");

            var key = new MappingKey(type, instanceName);

            if (transientMappings.ContainsKey(key) || transientMappings.ContainsKey(key))
            {
                const string errorMessageFormat = "The requested mapping already exists - {0}";
                throw new InvalidOperationException(string.Format(errorMessageFormat, key.ToTraceString()));
            }

            singletonMappings.Add(key, new Lazy<object>(createInstanceDelegate, true));
            AddMapping(type, key);
        }

        /// <summary>
        /// Check if a particular type/instance name has been registered with the container
        /// </summary>
        /// <param name="type">Type to check registration for</param>
        /// <param name="instanceName">Instance name (optional)</param>
        /// <returns><c>true</c>if the type/instance name has been registered 
        /// with the container; otherwise <c>false</c></returns>
        public bool IsRegistered(Type type, string instanceName = null)
        {
            if (type == null)
                throw new ArgumentNullException("type");


            var key = new MappingKey(type, instanceName);

            return transientMappings.ContainsKey(key);
        }

        /// <summary>
        /// Resolve an instance of the requested type from the container.
        /// </summary>
        /// <param name="type">Requested type</param>
        /// <param name="instanceName">Instance name (optional)</param>
        /// <returns>The retrieved object</returns>
        public object Resolve(Type type, string instanceName = null)
        {
            var key = new MappingKey(type, instanceName);
            var instance = GetInstance(key);
            if (instance != null)
            {
                return instance;
            }
            else
            {
                const string errorMessageFormat = "Could not find mapping for type '{0}'";
                throw new InvalidOperationException(string.Format(errorMessageFormat, type.FullName));
            }
        }

        private object GetInstance(MappingKey key)
        {
            Lazy<object> createSingletonInstance;
            Func<object> createTransientInstance;
            if (singletonMappings.TryGetValue(key, out createSingletonInstance))
            {
                return createSingletonInstance.Value;
            }
            else if (transientMappings.TryGetValue(key, out createTransientInstance))
            {
                return createTransientInstance();
            }
            else
                return null;
        }

        public List<T> ResolveAll<T>()
        {
            List<T> instances = new List<T>();
            List<MappingKey> mapsForCurrentType;
            if (mappings.TryGetValue(typeof(T), out mapsForCurrentType))
            {
                mapsForCurrentType.ForEach(m => instances.Add((T)GetInstance(m)));
            }
            return instances;
        }

        /// <summary>
        /// For debugging purposes only
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (transientMappings == null)
                return "No mappings";

            return string.Join(Environment.NewLine, transientMappings.Keys);
        }

        public void Destroy()
        {
            foreach (var item in singletonMappings)
            {
                var toDispose = item.Value.Value as IDisposable;
                if (toDispose != null)
                    toDispose.Dispose();
            }

            singletonMappings.Clear();
            transientMappings.Clear();
            mappings.Clear();
        }
    }
}
