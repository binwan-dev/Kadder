using System.Collections.Generic;
using Grpc.Core;

namespace Kadder
{
    public class GrpcServerOptions
    {
        public GrpcServerOptions()
        {
            IsGeneralProtoFile = true;
            ChannelOptions = new List<ChannelOption>();
            Ports = new List<ServerPort>();
        }

        public string PackageName { get; set; }

        public bool IsGeneralProtoFile { get; set; }

        public List<ChannelOption> ChannelOptions { get; }

        public List<ServerPort> Ports { get; }
    }
}
