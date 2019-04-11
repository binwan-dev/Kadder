using System;
using Atlantis.Grpc.Logging;
using Atlantis.Grpc.Utilies;

namespace Atlantis.Grpc
{
    public class GrpcConfiguration
    {
        static GrpcConfiguration()
        {
            ObjectContainer = new AutofacObjectContainer();
            JsonSerializer = new NewtonsoftJsonSerializer();
            BinarySerializer = new ProtobufBinarySerializer();
            LoggerFunc = t => new ConsoleLogger(t.FullName);
        }

        public static IObjectContainer ObjectContainer { get; set; }

        public static IJsonSerializer JsonSerializer { get; set; }

        public static IBinarySerializer BinarySerializer { get; set; }

        public static Func<Type, ILogger> LoggerFunc { get; set; }
    }
}
