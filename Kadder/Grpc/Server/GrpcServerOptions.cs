using System;
using System.Collections.Generic;
using Grpc.Core;

namespace Kadder
{
    public class GrpcServerOptions
    {
        public GrpcServerOptions()
        {
            PackageName = string.Empty;
            IsGeneralProtoFile = true;
            ChannelOptions = new List<ChannelOption>();
            Ports = new List<GrpcServerPort>();
        }

        public string PackageName { get; set; }

        public bool IsGeneralProtoFile { get; set; }

        public List<ChannelOption> ChannelOptions { get; }

        public List<GrpcServerPort> Ports { get; }

    }

    public class GrpcServerPort
    {
        public GrpcServerPort()
        {
            Name = string.Empty;
            Host = "0.0.0.0";
            Port = 1666;
            Credentials = ServerCredentials.Insecure;
        }

        public string Name { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public ServerCredentials Credentials { get; set; }
    }
}
