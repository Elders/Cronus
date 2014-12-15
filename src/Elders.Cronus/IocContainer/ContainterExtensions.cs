using System;

namespace Elders.Cronus.IocContainer
{
    public static class ContainterExtensions
    {
        /// <summary>
        /// Registers a type with transient life style.
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
        /// Registers a type with transient life style
        /// </summary>
        /// <typeparam name="TFrom">Type that will be requested</typeparam>
        /// <typeparam name="TTo">Type that will actually be returned</typeparam>
        /// <param name="instanceName">Instance name (optional)</param>
        public static void RegisterTransient<TFrom, TTo>(this IContainer self, string instanceName = null) where TTo : TFrom
        {
            self.RegisterTransient(typeof(TFrom), typeof(TTo), instanceName);
        }

        /// <summary>
        /// Registers a type with transient life style.
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
        /// Registers a type with singleton life style.
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
        /// Registers a type with singleton life style.
        /// </summary>
        /// <typeparam name="TFrom">Type that will be requested.</typeparam>
        /// <typeparam name="TTo">Type that will actually be returned.</typeparam>
        /// <param name="instanceName">Instance name (optional).</param>
        public static void RegisterSingleton<TFrom, TTo>(this IContainer self, string instanceName = null) where TTo : TFrom
        {
            self.RegisterSingleton(typeof(TFrom), typeof(TTo), instanceName);
        }

        /// <summary>
        /// Registers a type with singleton life style.
        /// </summary>
        /// <typeparam name="T">Type that will be requested.</typeparam>
        /// <param name="createInstanceDelegate">A delegate that will be used to create an instance of the requested object.</param>
        /// <param name="instanceName">Instance name (optional).</param>
        public static void RegisterSingleton<T>(this IContainer self, Func<T> createInstanceDelegate, string instanceName = null)
        {
            if (createInstanceDelegate == null)
                throw new ArgumentNullException("createInstanceDelegate");

            self.RegisterSingleton(typeof(T), createInstanceDelegate as Func<object>, instanceName);
        }

        /// <summary>
        /// Registers a type with scoped life style.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="scopeType">Type of the scope.</param>
        /// <exception cref="System.ArgumentNullException">to</exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        public static void RegisterScoped(this IContainer self, Type from, Type to, ScopeType scopeType = null)
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

            self.RegisterScoped(from, () => FastActivator.CreateInstance(to), scopeType);
        }


        /// <summary>
        /// Registers a type with scoped life style.
        /// </summary>
        /// <typeparam name="TFrom">Type that will be requested</typeparam>
        /// <typeparam name="TTo">Type that will actually be returned</typeparam>
        /// <param name="scopeType">Instance name (optional)</param>
        public static void RegisterScoped<TFrom, TTo>(this IContainer self, ScopeType scopeType = null) where TTo : TFrom
        {
            self.RegisterScoped(typeof(TFrom), typeof(TTo), scopeType);
        }

        /// <summary>
        /// Registers a type with scoped life style.
        /// </summary>
        /// <typeparam name="T">Type that will be requested</typeparam>
        /// <param name="createInstanceDelegate">A delegate that will be used to 
        /// create an instance of the requested object</param>
        /// <param name="scopeType">Type of the scope.</param>
        public static void RegisterScoped<T>(this IContainer self, Func<T> createInstanceDelegate, ScopeType scopeType = null)
        {
            if (createInstanceDelegate == null)
                throw new ArgumentNullException("createInstanceDelegate");

            self.RegisterScoped(typeof(T), createInstanceDelegate as Func<object>, scopeType);
        }

        public static void RegisterScoped(this IContainer self, Type type, Func<object> createInstanceDelegate, ScopeType scopeType = null)
        {
            if (createInstanceDelegate == null)
                throw new ArgumentNullException("createInstanceDelegate");

            self.RegisterScoped(type, createInstanceDelegate as Func<object>, scopeType);
        }

        /// <summary>
        /// Check if a particular type/instance name has been registered with the container
        /// </summary>
        /// <typeparam name="T">Type to check registration for</typeparam>
        /// <param name="scopeType">Type of the scope.</param>
        /// <returns><c>true</c>if the type/instance name has been registered with the container; otherwise <c>false</c></returns>
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

        public static IDisposable BeginScope(this IContainer self, ScopeType scopeType = null)
        {
            return self.BeginScope(scopeType);
        }
    }

    public sealed class ScopeType
    {
        private readonly String name;

        public static readonly ScopeType PerBatch = new ScopeType("perbatch");
        public static readonly ScopeType PerMessage = new ScopeType("permessage");
        public static readonly ScopeType PerHandler = new ScopeType("perhandler");
        public static readonly ScopeType Default = new ScopeType(null);

        private ScopeType(string name)
        {
            this.name = name;
        }

        public override String ToString()
        {
            return name;
        }

        public static implicit operator string (ScopeType scopeType)
        {
            ScopeType theScopeType = scopeType == null ? ScopeType.Default : scopeType;
            return theScopeType.ToString();
        }

    }
}
