using System;
using Followme.AspNet.Core.FastCommon.Components;

namespace Followme.AspNet.Core.FastCommon.ThirdParty.GrpcServer.Middlewares
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
