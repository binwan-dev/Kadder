using Kadder.Grpc.Logger.Interceptor;
using Kadder.Grpc.Server.AspNetCore;

namespace Microsoft.Extensions.Hosting
{
    public static class KadderLoggerGrpcInterceptorExtension
    {
        public static GrpcServerBuilder AddLoggerInterceptor(this GrpcServerBuilder builder)
        {
            builder.AddInterceptor<ServiceLoggerInterceptor>();
            return builder;
        }
    }
}