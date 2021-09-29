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
        // public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        // {
        // }

        // private async Task<TResponse> HandleAsync<TResponse>(AsyncUnaryCall<TResponse> response)
        // {
        //     try
        //     {
        //         var result = await response.ResponseAsync;
        //         Console.WriteLine("sss");
        //         return result;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("ssssss");
        //         throw ex;
        //     }
        // }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            return base.AsyncServerStreamingCall(request, context, continuation);
        }

        // public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        // {
        //     var responseCon = continuation(context);
        //     var response = new AsyncUnaryCall<TResponse>(Handler(responseCon), responseCon.ResponseHeadersAsync, responseCon.GetStatus, responseCon.GetTrailers, responseCon.Dispose);
        //     // Console.WriteLine($"客户端调用执行结束{DateTime.Now.ToString("ss.fff")} ");
        //     return responseCon;
        // }

        private async Task<TResponse> Handler<TRequest, TResponse>(AsyncClientStreamingCall<TRequest, TResponse> response)
        {
            try
            {
                Console.WriteLine("start");
                var result = await response.ResponseAsync;
                Console.WriteLine("end");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("client interceptor for client stream");
                throw ex;
            }
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            return base.AsyncDuplexStreamingCall(context, continuation);
        }

    }
}
