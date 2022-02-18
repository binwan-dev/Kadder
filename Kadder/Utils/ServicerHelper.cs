using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kadder.Utilies;

namespace Kadder.Utils
{
    public static class ServicerHelper
    {
        public static List<Type> GetServicerTypes(List<Assembly> assemblies)
        {
            var kServicers = new List<Type>();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetModules()[0].GetTypes();
                kServicers.AddRange(
                    types.Where(p => p.GetInterface(typeof(IMessagingServicer).Name) != null ||
                                p.IsSubclassOf(typeof(KServicer)) ||
                                p.IsAssignableFrom(typeof(KServicer)) ||
                                p.Name.EndsWith("KServicer") ||
                                p.CustomAttributes.Count(x => x.AttributeType == typeof(KServicerAttribute)) > 0));
            }
            return kServicers;

        }

        public static MethodInfo[] GetMethod(Type servicerType)
        {
            return servicerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }
    }
}
