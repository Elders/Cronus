//using System;
//using System.Linq;
//using System.Reflection;

//namespace NMSD.Cronus
//{
//    public static class ReflectionExtensions
//    {
//        public static T GetAssemblyAttribute<T>(this Type type)
//        {
//            return GetAssemblyAttribute<T>(type.Assembly);
//        }

//        public static T GetAssemblyAttribute<T>(this Assembly assembly)
//        {
//            var attributeType = typeof(T);
//            var attribute = assembly
//                .GetCustomAttributes(attributeType, false)
//                .SingleOrDefault();

//            return attribute == null
//                ? default(T)
//                : (T)attribute;
//        }
//    }
//}