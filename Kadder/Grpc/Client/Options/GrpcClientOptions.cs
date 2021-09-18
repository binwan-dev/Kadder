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
            Addresses = new List<ChannelOption>();
            Assemblies = new List<Assembly>();
            AssemblyFullNames = new List<string>();
            ConnectSecondTimeout = 10;
            KeepLive = true;
        }

        public IList<ChannelOption> Addresses { get; set; }

        /// <summary>
        /// Connection timeout (unit: s)
        /// </summary>
        public int ConnectSecondTimeout { get; set; }

        /// <summary>
        /// Keep connect live
        /// </summary>
        public bool KeepLive { get; set; }

        public IList<Assembly> Assemblies { get; set; }

        public IList<string> AssemblyFullNames { get; set; }
    }
}
