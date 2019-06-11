using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kadder.Middlewares
{
    public class GrpcHandlerDirector
    {
        private static IList<Type> _thirdMiddlewares;

        static GrpcHandlerDirector()
        {
            _thirdMiddlewares=new List<Type>();
        }

        public static void AddMiddleware(Type middleware)
        {
            _thirdMiddlewares.Add(middleware);
        }
        
        public static void ConfigActor()
        {
            var handlerBuilder=GrpcServerBuilder.ServiceProvider.GetService<GrpcHandlerBuilder>();
            handlerBuilder.UseMiddleware<LoggerMiddleware>();
            foreach(var item in _thirdMiddlewares)
            {
                handlerBuilder.UseMiddleware(item);
            }
            handlerBuilder.UseMiddleware<HandlerSelectMiddleware>().Build();
        }
    }
}
