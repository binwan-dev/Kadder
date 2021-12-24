using System;
using System.Collections.Generic;
using System.Reflection;
using Grpc.Core.Interceptors;

namespace Kadder.Grpc.Client.Options
{
    public class GrpcProxyerOptions
    {
        public GrpcProxyerOptions()
        {
            Name = string.Empty;
            PackageName = string.Empty;
            Addresses = new List<GrpcChannelOptions>();
            Assemblies = new List<Assembly>();
            Interceptors = new List<Type>();
            AssemblyNames = new List<string>();
            ConnectSecondTimeout = 10;
            KeepLive = true;
        }

        public string Name { get; set; }

        public string PackageName { get; set; }

        public IList<GrpcChannelOptions> Addresses { get; set; }

        /// <summary>
        /// Connection timeout (unit: s)
        /// </summary>
        public int ConnectSecondTimeout { get; set; }

        /// <summary>
        /// Keep connect live
        /// </summary>
        public bool KeepLive { get; set; }

        internal List<Assembly> Assemblies { get; set; }

        internal List<Type> Interceptors { get; set; }

        public List<string> AssemblyNames { get; set; }

        public GrpcProxyerOptions AddAssembly(params Assembly[] assemblies)
        {
            Assemblies.AddRange(assemblies);
            return this;
        }

        public GrpcProxyerOptions AddInterceptor<Interceptor>()
        {
            Interceptors.Add(typeof(Interceptor));
            return this;
        }
    }
}
