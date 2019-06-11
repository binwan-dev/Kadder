using System;
using System.Collections.Generic;
using Kadder.Middlewares;
using Kadder.Utilies;
using Microsoft.Extensions.Options;

namespace Kadder
{
    public class GrpcServerBuilder
    {
        public GrpcServerBuilder()
        {
            Middlewares=new List<Type>();
        }
        
        public GrpcServerOptions Options{get;set;}

        public IJsonSerializer JsonSerializer{get;set;}

        public IBinarySerializer BinarySerializer{get;set;}

        internal IList<Type> Middlewares{get;private set;}

        public static IServiceProvider ServiceProvider{get;set;}

        public GrpcServerBuilder AddMiddleware<T>()where T:GrpcMiddlewareBase
        {
            Middlewares.Add(typeof(T));
            return this;
        }

    }
}
