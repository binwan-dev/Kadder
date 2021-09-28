using Grpc.Core;
using Grpc.Core.Interceptors;
using Kadder.Simple.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kadder.Simple.Client
{
    public class LoggerInterceptor : Interceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            Console.WriteLine($"客户端调用执行开始 {DateTime.Now.ToString("ss.fff")}");
            Console.WriteLine(typeof(TRequest).FullName);
            AsyncUnaryCall<TResponse> responseCon;
            if (request is HelloMessage helloRequest)
            {
                // helloRequest.Name = "inter";
                responseCon = continuation(request, context);
            }
            else
            {
                responseCon = continuation(request, context);
            }
            // var result = responseCon.ResponseAsync.Result;
            // var response = new AsyncUnaryCall<TResponse>(responseCon.ResponseAsync, responseCon.ResponseHeadersAsync, responseCon.GetStatus, responseCon.GetTrailers, responseCon.Dispose);
            Console.WriteLine($"客户端调用执行结束{DateTime.Now.ToString("ss.fff")} ");
            return responseCon;
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            return base.AsyncServerStreamingCall(request, context, continuation);
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            Console.WriteLine($"客户端调用执行开始 {DateTime.Now.ToString("ss.fff")}");
            Console.WriteLine(typeof(TRequest).FullName);
            // throw new Exception("ss");
            var responseCon = continuation(context);
            // var response = new AsyncUnaryCall<TResponse>(responseCon.ResponseAsync, responseCon.ResponseHeadersAsync, responseCon.GetStatus, responseCon.GetTrailers, responseCon.Dispose);
            Console.WriteLine($"客户端调用执行结束{DateTime.Now.ToString("ss.fff")} ");
            return responseCon;
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            return base.AsyncDuplexStreamingCall(context, continuation);
        }

    }
}
