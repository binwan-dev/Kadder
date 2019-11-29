using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kadder.Utilies;

namespace Kadder
{
    public class GrpcOptions
    {
        public GrpcOptions()
        {
            ScanAssemblies = new string[]
            {
                Assembly.GetEntryAssembly().FullName
            };
        }

        public string Host { get; set; }

        public int Port { get; set; }

        public string NamespaceName { get; set; }

        public string ServiceName { get; set; }

        public string[] ScanAssemblies { get; set; }

        public Assembly[] GetScanAssemblies()
        {
            var assemblies = new List<Assembly>();
            foreach (var item in ScanAssemblies)
            {
                assemblies.Add(Assembly.Load(item));
            }
            return assemblies.ToArray();
        }

        public Type[] GetKServicers()
        {
            var kServicers = new List<Type>();
            foreach (var assembly in GetScanAssemblies())
            {
                var types = assembly.GetModules()[0].GetTypes();
                kServicers.AddRange(
                    types.Where(p => p.GetInterface(typeof(IMessagingServicer).Name) != null ||
                                p.IsSubclassOf(typeof(KServicer)) ||
                                p.IsAssignableFrom(typeof(KServicer)) ||
                                p.Name.EndsWith("KServicer") ||
                                p.CustomAttributes.Count(x => x.AttributeType == typeof(KServicerAttribute)) > 0));
            }
            return kServicers.ToArray();
        }

    }
}
