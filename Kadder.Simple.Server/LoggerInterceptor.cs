using Grpc.Core;
using Grpc.Core.Interceptors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kadder.Simple.Server
{
    public class LoggerInterceptor: Interceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            Console.WriteLine($"server handle start ,request :{JsonConvert.SerializeObject(request)}...");
            var response = await continuation(request, context);
            Console.WriteLine($"server handle finish, response:{JsonConvert.SerializeObject(response)}");
            return response;
        }
    }
}
