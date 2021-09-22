using System;
using System.Collections.Generic;
using System.Reflection;
using Grpc.Core;

namespace Kadder.Grpc.Client.Options
{
    public class GrpcClientOptions
    {
        public GrpcClientOptions()
        {
            Addresses = new List<GrpcChannelOptions>();
            Assemblies = new List<Assembly>();
            ConnectSecondTimeout = 10;
            KeepLive = true;
        }

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

        public GrpcClientOptions AddAssembly(params Assembly[] assemblies)
        {
            Assemblies.AddRange(assemblies);
            return this;
        }
    }
}
