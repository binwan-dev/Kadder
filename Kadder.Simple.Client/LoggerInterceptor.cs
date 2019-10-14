using Grpc.Core;
using Grpc.Core.Interceptors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kadder.Simple.Client
{
    public class LoggerInterceptor : Interceptor
    {
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            Console.WriteLine($"client call start ,request :{JsonConvert.SerializeObject(request)}...");
            var response = continuation(request, context);
            Console.WriteLine($"client call finish, response:{JsonConvert.SerializeObject(response)}");
            return response;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            Console.WriteLine($"client call start ,request :{JsonConvert.SerializeObject(request)}...");
            var response = continuation(request, context);
            var responseAsync = response.ResponseAsync.ContinueWith<TResponse>(r =>
            {
                Console.WriteLine($"client async call finish, response:{JsonConvert.SerializeObject(r.Result)}");
                return r.Result;
            });
            return new AsyncUnaryCall<TResponse>(responseAsync, response.ResponseHeadersAsync, response.GetStatus, response.GetTrailers, response.Dispose);
        }
    }
}
