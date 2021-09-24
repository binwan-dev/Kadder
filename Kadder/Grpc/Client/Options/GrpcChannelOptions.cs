using Grpc.Core;

namespace Kadder.Grpc.Client.Options
{
    public class GrpcChannelOptions
    {
        public GrpcChannelOptions()
        {
            Name = Address;
            Credentials = ChannelCredentials.Insecure;
        }

        public string Name { get; set; }

        public string Address { get; set; }

        public ChannelCredentials Credentials { get; set; }
    }
}
