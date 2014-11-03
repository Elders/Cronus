using System;

namespace Elders.Cronus.IocContainer
{
    public static class ContainterExtensions
    {
        /// <summary>
        /// Register a type with transient life style
        /// </summary>
        /// <param name="from">Type that will be requested</param>
        /// <param name="to">Type that will actually be returned</param>
        /// <param name="instanceName">Instance name (optional)</param>
        public static void RegisterTransient(this IContainer self, Type from, Type to, string instanceName = null)
        {
            //if (from == null)
            //	throw new ArgumentNullException("from");

            if (to == null)
                throw new ArgumentNullException("to");

            if (!from.IsAssignableFrom(to))
            {
                const string errorMessageFormat = "Error trying to register the instance: '{0}' is not assignable from '{1}'";
                throw new InvalidOperationException(string.Format(errorMessageFormat, from.FullName, to.FullName));
            }


            self.RegisterTransient(from, () => FastActivator.CreateInstance(to), instanceName);
        }


        /// <summary>
        /// Register a type with transient life style
        /// </summary>
        /// <typeparam name="TFrom">Type that will be requested</typeparam>
        /// <typeparam name="TTo">Type that will actually be returned</typeparam>
        /// <param name="instanceName">Instance name (optional)</param>
        public static void RegisterTransient<TFrom, TTo>(this IContainer self, string instanceName = null) where TTo : TFrom
        {
            self.RegisterTransient(typeof(TFrom), typeof(TTo), instanceName);
        }

        /// <summary>
        /// Register a type with transient life style
        /// </summary>
        /// <typeparam name="T">Type that will be requested</typeparam>
        /// <param name="createInstanceDelegate">A delegate that will be used to 
        /// create an instance of the requested object</param>
        /// <param name="instanceName">Instance name (optional)</param>
        public static void RegisterTransient<T>(this IContainer self, Func<T> createInstanceDelegate, string instanceName = null)
        {
            if (createInstanceDelegate == null)
                throw new ArgumentNullException("createInstanceDelegate");


            self.RegisterTransient(typeof(T), createInstanceDelegate as Func<object>, instanceName);
        }

        /// <summary>
        /// Register a type with singleton life style
        /// </summary>
        /// <param name="from">Type that will be requested</param>
        /// <param name="to">Type that will actually be returned</param>
        /// <param name="instanceName">Instance name (optional)</param>
        public static void RegisterSingleton(this IContainer self, Type from, Type to, string instanceName = null)
        {
            //if (from == null)
            //	throw new ArgumentNullException("from");

            if (to == null)
                throw new ArgumentNullException("to");

            if (!from.IsAssignableFrom(to))
            {
                const string errorMessageFormat = "Error trying to register the instance: '{0}' is not assignable from '{1}'";
                throw new InvalidOperationException(string.Format(errorMessageFormat, from.FullName, to.FullName));
            }


            self.RegisterSingleton(from, () => FastActivator.CreateInstance(to), instanceName);
        }


        /// <summary>
        /// Register a type with singleton life style
        /// </summary>
        /// <typeparam name="TFrom">Type that will be requested</typeparam>
        /// <typeparam name="TTo">Type that will actually be returned</typeparam>
        /// <param name="instanceName">Instance name (optional)</param>
        public static void RegisterSingleton<TFrom, TTo>(this IContainer self, string instanceName = null) where TTo : TFrom
        {
            self.RegisterSingleton(typeof(TFrom), typeof(TTo), instanceName);
        }

        /// <summary>
        /// Register a type with singleton life style
        /// </summary>
        /// <typeparam name="T">Type that will be requested</typeparam>
        /// <param name="createInstanceDelegate">A delegate that will be used to 
        /// create an instance of the requested object</param>
        /// <param name="instanceName">Instance name (optional)</param>
        public static void RegisterSingleton<T>(this IContainer self, Func<T> createInstanceDelegate, string instanceName = null)
        {
            if (createInstanceDelegate == null)
                throw new ArgumentNullException("createInstanceDelegate");


            self.RegisterSingleton(typeof(T), createInstanceDelegate as Func<object>, instanceName);
        }

        /// <summary>
        /// Check if a particular type/instance name has been registered with the container
        /// </summary>
        /// <typeparam name="T">Type to check registration for</typeparam>
        /// <param name="instanceName">Instance name (optional)</param>
        /// <returns><c>true</c>if the type/instance name has been registered 
        /// with the container; otherwise <c>false</c></returns>
        public static bool IsRegistered<T>(this IContainer self, string instanceName = null)
        {
            return self.IsRegistered(typeof(T), instanceName);
        }

        /// <summary>
        /// Resolve an instance of the requested type from the container.
        /// </summary>
        /// <typeparam name="T">Requested type</typeparam>
        /// <param name="instanceName">Instance name (optional)</param>
        /// <returns>The retrieved object</returns>
        public static T Resolve<T>(this IContainer self, string instanceName = null)
        {
            object instance = self.Resolve(typeof(T), instanceName);

            return (T)instance;
        }
    }
}
