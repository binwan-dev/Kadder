using Grpc.Core;

namespace Followme.AspNet.Core.FastCommon.ThirdParty.GrpcServer
{
    public interface IGrpcServices
    {
        ServerServiceDefinition BindServices();
    } 
}
