using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Kadder.Simple.Client
{
    public class LoggerInterceptor : Interceptor
    {
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            Console.WriteLine($"client call start ,request :{JsonSerializer.Serialize(request)}...");
            var response = continuation(request, context);
            Console.WriteLine($"client call finish, response:{JsonSerializer.Serialize(response)}");
            return response;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            Console.WriteLine($"client call start ,request :{JsonSerializer.Serialize(request)}...");
            var response = continuation(request, context);
            var responseAsync = response.ResponseAsync.ContinueWith<TResponse>(r =>
            {
                Console.WriteLine($"client async call finish, response:{JsonSerializer.Serialize(r.Result)}");
                return r.Result;
            });
            return new AsyncUnaryCall<TResponse>(responseAsync, response.ResponseHeadersAsync, response.GetStatus, response.GetTrailers, response.Dispose);
        }
    }
}
