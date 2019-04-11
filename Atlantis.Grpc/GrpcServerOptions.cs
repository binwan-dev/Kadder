using System.Reflection;
using Atlantis.Grpc.Utilies;

namespace Atlantis.Grpc
{
    public class GrpcServerOptions:GrpcOptions
    {
        public GrpcServerOptions()
        {
            IsGeneralProtoFile=true;
        }

        public string PackageName{get;set;}
        
        public bool IsGeneralProtoFile{get;set;}
    }
}
