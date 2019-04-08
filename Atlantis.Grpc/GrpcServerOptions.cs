using System.Reflection;
using Atlantis.Grpc.Utilies;

namespace Atlantis.Grpc
{
    public class GrpcServerOptions
    {
        public GrpcServerOptions()
        {
            ObjectContainer=new AutofacObjectContainer();
            JsonSerializer=new NewtonsoftJsonSerializer();
            BinarySerializer=new ProtobufBinarySerializer();
            IsGeneralProtoFile=true;
        }
        
        public string Host{get;set;}

        public int Port{get;set;}

        public string NamespaceName{get;set;}

        public string PackageName{get;set;}

        public string ServiceName{get;set;}

        public bool IsGeneralProtoFile{get;set;}

        public Assembly[] ScanAssemblies{get;set;}

        public IObjectContainer ObjectContainer{get;set;}

        public IJsonSerializer JsonSerializer{get;set;}

        public IBinarySerializer BinarySerializer{get;set;}
    }
}
