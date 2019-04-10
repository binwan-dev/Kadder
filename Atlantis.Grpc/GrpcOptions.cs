using System.Reflection;
using Atlantis.Grpc.Logging;
using Atlantis.Grpc.Utilies;

namespace Atlantis.Grpc
{
    public class GrpcOptions
    {
        public GrpcOptions()
        {
            ObjectContainer=new AutofacObjectContainer();
            JsonSerializer=new NewtonsoftJsonSerializer();
            BinarySerializer=new ProtobufBinarySerializer();
            LoggerFactory=new NullLoggerFactory();
        }
        
        public string Host{get;set;}

        public int Port{get;set;}

        public string NamespaceName{get;set;}

        public string PackageName{get;set;}

        public string ServiceName{get;set;}

        public Assembly[] ScanAssemblies{get;set;}

        public IObjectContainer ObjectContainer{get;set;}

        public IJsonSerializer JsonSerializer{get;set;}

        public IBinarySerializer BinarySerializer{get;set;}

        public ILoggerFactory LoggerFactory{get;set;}
    }
}
