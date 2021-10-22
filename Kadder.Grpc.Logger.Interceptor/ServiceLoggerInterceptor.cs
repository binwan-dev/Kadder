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

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var methodName = $"{continuation.Method.DeclaringType.Name} -> {continuation.Method.Name}";
            try
            {
                _logger.LogInformation($"Method Name:{methodName}, Receive Request: {JsonConvert.SerializeObject(request)}");

                var time = DateTime.Now;
                var response = await continuation(request, context);
                var doneTime = DateTime.Now;

                var usedTime = (doneTime - time).TotalMilliseconds;
                _logger.LogInformation($"Method({methodName}) handle complete! used time: {usedTime}ms, Response: {JsonConvert.SerializeObject(response)}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Method({methodName}) handle exception! Request: {JsonConvert.SerializeObject(request)}");
                throw ex;
            }
        }
    }
}