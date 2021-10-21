using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using GrpcInterceptor = Grpc.Core.Interceptors.Interceptor;

namespace Kadder.Grpc.Logger.Interceptor
{
    public class ServiceLoggerInterceptor : GrpcInterceptor
    {
        private readonly ILogger<ServiceLoggerInterceptor> _logger;

        public ServiceLoggerInterceptor(ILogger<ServiceLoggerInterceptor> logger)
        {
            _logger = logger;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, global::Grpc.Core.Interceptors.ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            Console.WriteLine("sss");
            return base.AsyncUnaryCall(request, context, continuation);
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                _logger.LogInformation($"Method Name:{continuation.Method.Name}, Receive Request: {JsonConvert.SerializeObject(request)}");

                var time = DateTime.Now;
                var response = await continuation(request, context);
                var doneTime = DateTime.Now;

                var usedTime = (doneTime - time).TotalMilliseconds;
                _logger.LogInformation($"Method({continuation.Method.Name}) handle complete! used time: {usedTime}ms, Response: {JsonConvert.SerializeObject(response)}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Method({continuation.Method.Name}) handle exception! Request: {JsonConvert.SerializeObject(request)}");
                throw ex;
            }
        }
    }
}