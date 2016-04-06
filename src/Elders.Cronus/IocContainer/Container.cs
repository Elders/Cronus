using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

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
        private readonly Dictionary<MappingKey, Func<object>> scopedMappings;
        private readonly Dictionary<Type, List<MappingKey>> mappings;

        /// <summary>
        /// Creates a new instance of <see cref="Container"/>
        /// </summary>
        public Container()
        {
            this.transientMappings = new Dictionary<MappingKey, Func<object>>();
            this.singletonMappings = new Dictionary<MappingKey, Lazy<object>>();
            this.scopedMappings = new Dictionary<MappingKey, Func<object>>();
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
            Guard_AlreadyRegistered(key);

            transientMappings.Add(key, createInstanceDelegate);
            AddMapping(type, key);
        }

        private void AddMapping(Type type, MappingKey key)
        {
            List<MappingKey> mapsForCurrentType;
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
            Guard_AlreadyRegistered(key);

            singletonMappings.Add(key, new Lazy<object>(createInstanceDelegate, true));
            AddMapping(type, key);
        }

        public void RegisterScoped(Type type, Func<object> createInstanceDelegate, string scopeType = null)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (createInstanceDelegate == null) throw new ArgumentNullException("createInstanceDelegate");

            var key = new MappingKey(type, scopeType);
            Guard_AlreadyRegistered(key);

            scopedMappings.Add(key, createInstanceDelegate);
        }

        private void Guard_AlreadyRegistered(MappingKey key)
        {
            if (singletonMappings.ContainsKey(key) || transientMappings.ContainsKey(key) || scopedMappings.ContainsKey(key))
            {
                const string errorMessageFormat = "The requested mapping already exists - {0}";
                throw new InvalidOperationException(string.Format(errorMessageFormat, key.ToTraceString()));
            }
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

            return transientMappings.ContainsKey(key) || singletonMappings.ContainsKey(key) || scopedMappings.ContainsKey(key);
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
            object instance = null;
            Lazy<object> createSingletonInstance;
            Func<object> createTransientInstance;
            if (Scope.IsInScope)
            {
                instance = Scope.GetCurrentScope().GetInstance(key);
            }

            if (instance == null && singletonMappings.TryGetValue(key, out createSingletonInstance))
            {
                instance = createSingletonInstance.Value;
            }
            else if (instance == null && transientMappings.TryGetValue(key, out createTransientInstance))
            {
                instance = createTransientInstance();
            }

            return instance;
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
            scopedMappings.Clear();
            mappings.Clear();
        }

        public IDisposable BeginScope(string scopeType)
        {
            return new Scope(scopedMappings, scopeType);
        }
    }

    internal class Scope : IDisposable
    {
        private readonly Dictionary<MappingKey, Lazy<object>> scopedMappings;

        public Scope(Dictionary<MappingKey, Func<object>> mappings, string scopeType)
        {
            ScopeId = Guid.NewGuid();
            ScopeType = scopeType;

            this.scopedMappings = IsInScope
                ? new Dictionary<MappingKey, Lazy<object>>(GetCurrentScope().scopedMappings)
                : mappings.ToDictionary(key => key.Key, value => new Lazy<object>(value.Value, true));
            Scope.ScopeHolder.SetCurrentScope(this);
        }

        public string ScopeType { get; private set; }
        public Guid ScopeId { get; private set; }
        public Guid ParentScopeId { get; private set; }

        public bool IsRoot
        {
            get { return ScopeId == ParentScopeId; }
        }

        internal object GetInstance(MappingKey key)
        {
            Lazy<object> createScopedInstance;
            if (scopedMappings.TryGetValue(key, out createScopedInstance))
            {
                return createScopedInstance.Value;
            }
            else
            {
                if (String.IsNullOrEmpty(key.InstanceName))
                {
                    var scopedKey = new MappingKey(key.Type, Scope.GetCurrentScope().ScopeType);
                    if (scopedMappings.TryGetValue(scopedKey, out createScopedInstance))
                    {
                        return createScopedInstance.Value;
                    }
                    else
                    {
                        //  If we do cannot create an instance there is a possibility the requested key to be registered in a parent scope.
                        //  If that is the case the instance is created within the correct scope without switching the current scope.
                        Scope scopeContainingMappingKey;
                        MappingKey keyToResolveFromFoundScope;
                        if (ScopeHolder.TryFindWithinParents(scopedKey, ScopeHolder.CurrentScope, out scopeContainingMappingKey, out keyToResolveFromFoundScope))
                        {
                            return scopeContainingMappingKey.GetInstance(keyToResolveFromFoundScope);
                        }
                    }
                }
            }
            return null;
        }

        public void Dispose()
        {
            scopedMappings.Clear();
            ScopeHolder.EndScope();
        }

        public static bool IsInScope { get { return !ReferenceEquals(null, ScopeHolder.CurrentScope); } }

        public static Scope GetCurrentScope()
        {
            return ScopeHolder.CurrentScope;
        }

        class ScopeHolder
        {
            private static ConcurrentDictionary<Guid, Scope> scopes = new ConcurrentDictionary<Guid, Scope>();

            [ThreadStatic]
            public static Scope CurrentScope;

            public static void SetCurrentScope(Scope scope)
            {
                if (!IsInScope)
                    CurrentScope = scope;

                if (scopes.TryAdd(scope.ScopeId, scope))
                {
                    scope.ParentScopeId = CurrentScope.ScopeId;
                    CurrentScope = scope;
                }
                else
                {
                    throw new InvalidProgramException("TheScopeHolder.SetCurrentScope(scope)");
                }
            }

            public static bool TryFindWithinParents(MappingKey key, Scope startingScope, out Scope scopeContainingMappingKey, out MappingKey keyToResolveFromFoundScope)
            {
                scopeContainingMappingKey = null;
                keyToResolveFromFoundScope = key;
                Scope scopeToTest = startingScope;
                while (!scopeToTest.IsRoot)
                {
                    if (scopes.TryGetValue(scopeToTest.ParentScopeId, out scopeToTest))
                    {
                        keyToResolveFromFoundScope = new MappingKey(key.Type, scopeToTest.ScopeType);
                        if (scopeToTest.scopedMappings.ContainsKey(keyToResolveFromFoundScope))
                        {
                            scopeContainingMappingKey = scopeToTest;
                            return true;
                        }
                    }
                    else
                        break;
                }
                return false;
            }

            public static void EndScope()
            {
                Scope scope;
                if (scopes.TryRemove(CurrentScope.ScopeId, out scope))
                {
                    Scope parent;
                    if (scopes.TryGetValue(scope.ParentScopeId, out parent))
                        CurrentScope = parent;
                    else if (scope.IsRoot)
                        CurrentScope = null;
                    else
                    {
                        throw new InvalidProgramException("TheScopeHolder.EndScope()");
                    }
                }
                else
                {
                    throw new InvalidProgramException("TheScopeHolder.EndScope()");
                }
            }
        }
    }
}
