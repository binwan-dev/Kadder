using Grpc.Core;

namespace Atlantis.Grpc
{
    public interface IGrpcServices
    {
        ServerServiceDefinition BindServices();
    } 
}
