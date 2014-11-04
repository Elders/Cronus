using System;
using System.Collections.Generic;

namespace Elders.Cronus.IocContainer
{
    /// <summary>
    /// Simple IoC container
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// Register a type with transient life style
        /// </summary>
        /// <param name="type">Type that will be requested</param>
        /// <param name="createInstanceDelegate">A delegate that will be used to 
        /// create an instance of the requested object</param>
        /// <param name="instanceName">Instance name (optional)</param>
        void RegisterTransient(Type type, Func<object> createInstanceDelegate, string instanceName = null);

        /// <summary>
        /// Register a type with singleton life style
        /// </summary>
        /// <param name="type">Type that will be requested</param>
        /// <param name="createInstanceDelegate">A delegate that will be used to 
        /// create an instance of the requested object</param>
        /// <param name="instanceName">Instance name (optional)</param>
        void RegisterSingleton(Type type, Func<object> createInstanceDelegate, string instanceName = null);

        /// <summary>
        /// Check if a particular type/instance name has been registered with the container
        /// </summary>
        /// <param name="type">Type to check registration for</param>
        /// <param name="instanceName">Instance name (optional)</param>
        /// <returns><c>true</c>if the type/instance name has been registered 
        /// with the container; otherwise <c>false</c></returns>
        bool IsRegistered(Type type, string instanceName = null);

        /// <summary>
        /// Resolve an instance of the requested type from the container.
        /// </summary>
        /// <param name="type">Requested type</param>
        /// <param name="instanceName">Instance name (optional)</param>
        /// <returns>The retrieved object</returns>
        object Resolve(Type type, string instanceName = null);

        List<T> ResolveAll<T>();

        void Destroy();
    }
}
