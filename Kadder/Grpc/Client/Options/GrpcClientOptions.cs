using System;
using System.Collections.Generic;
using Grpc.Core;

namespace Kadder.Grpc.Client.Options
{
    public class GrpcClientOptions
    {
        public GrpcClientOptions()
        {
            Addresses=new List<string>();
            ConnectSecondTimeout = 10;
            KeepLive=true;
        }

        public IList<string> Addresses{get;set;}
        
        /// <summary>
        /// Connection timeout (unit: s)
        /// </summary>
        public int ConnectSecondTimeout { get; set; }

        /// <summary>
        /// Keep connect live
        /// </summary>
        public bool KeepLive { get; set; }
    }
}
