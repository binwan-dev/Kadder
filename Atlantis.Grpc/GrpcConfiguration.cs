using System;
using Atlantis.Grpc.Logging;
using Atlantis.Grpc.Utilies;

namespace Atlantis.Grpc
{
    public class GrpcConfiguration
    {

        public static IObjectContainer ObjectContainer;

        public static IJsonSerializer JsonSerializer;

        public static IBinarySerializer BinarySerializer;

        public static Func<Type, ILogger> LoggerFunc;

        static GrpcConfiguration()
        {
            ObjectContainer = new AutofacObjectContainer();
            JsonSerializer = new NewtonsoftJsonSerializer();
            BinarySerializer = new ProtobufBinarySerializer();
            LoggerFunc = GetDefaultLogger;
        }

        public static ILogger GetDefaultLogger(Type t)
        {
            return new ConsoleLogger(t.FullName); 
        }
    }
}
