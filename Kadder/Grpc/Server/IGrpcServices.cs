using System.Collections.Generic;
using Grpc.Core;

namespace Kadder
{
    public interface IGrpcServices
    {
        ServerServiceDefinition BindServices();
    } 
}
