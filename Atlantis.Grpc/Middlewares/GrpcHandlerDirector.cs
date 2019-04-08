using Atlantis.Grpc.Utilies;

namespace Atlantis.Grpc.Middlewares
{
    public class GrpcHandlerDirector
    {
        public static void ConfigActor()
        {
            var handlerBuilder=ObjectContainer.Resolve<GrpcHandlerBuilder>();
            handlerBuilder.UseMiddleware<LoggerMiddleware>().UseMiddleware<HandlerSelectMiddleware>().Build();
        }
    }
}
