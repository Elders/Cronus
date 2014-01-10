using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NMSD.Cronus
{
    public static class FastActivator
    {
        static ConcurrentDictionary<Type, ObjectActivator> activators = new ConcurrentDictionary<Type, ObjectActivator>();

        public delegate object ObjectActivator(params object[] args);

        static ObjectActivator GetActivator(ConstructorInfo ctor)
        {
            ParameterInfo[] paramsInfo = ctor.GetParameters();
            ParameterExpression param = Expression.Parameter(typeof(object[]), "args");
            Expression[] argsExp = new Expression[paramsInfo.Length];
            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;
                Expression paramAccessorExp = Expression.ArrayIndex(param, index);
                Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);
                argsExp[i] = paramCastExp;
            }
            NewExpression newExp = Expression.New(ctor, argsExp);
            LambdaExpression lambda = Expression.Lambda(typeof(ObjectActivator), newExp, param);
            return (ObjectActivator)lambda.Compile();
        }

        public static void WarmInstanceConstructor(Type type)
        {
            if (!activators.ContainsKey(type))
            {
                var constructors = type.GetConstructors();
                if (constructors.Length == 1)
                {
                    ConstructorInfo ctor = type.GetConstructors().First();
                    activators.TryAdd(type, GetActivator(ctor));
                }
            }
        }

        public static object CreateInstance(Type type, params object[] args)
        {
            ObjectActivator activator;
            if (!activators.TryGetValue(type, out activator))
            {
                var constructors = type.GetConstructors();
                if (constructors.Length == 1)
                {
                    ConstructorInfo ctor = constructors.First();
                    activator = GetActivator(ctor);
                    activators.TryAdd(type, activator);
                }
                else
                {
                    activator = (a) => Activator.CreateInstance(type, a);
                }

            }
            return activator(args);
        }
        public static object CreateInstance(Type type, bool @private, params object[] args)
        {
            ObjectActivator activator;
            if (!activators.TryGetValue(type, out activator))
            {
                var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
                if (constructors.Length == 1)
                {
                    ConstructorInfo ctor = constructors.First();
                    activator = GetActivator(ctor);
                    activators.TryAdd(type, activator);
                }
                else
                {
                    activator = (a) => Activator.CreateInstance(type, a);
                }

            }
            return activator(args);
        }
    }
}