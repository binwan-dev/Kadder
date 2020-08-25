using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Kadder.Utilies;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Kadder.Middlewares
{
    public class LoggerMiddleware : GrpcMiddlewareBase
    {
        private readonly ILogger<LoggerMiddleware> _logger;
        private readonly IDictionary<Guid, DateTime> _startCallTimerDic;

        public LoggerMiddleware(HandlerDelegateAsync next) : base(next)
        {
            var provider = GrpcServerBuilder.ServiceProvider;
            _logger = provider.GetService<ILogger<LoggerMiddleware>>();
            _startCallTimerDic = new Dictionary<Guid, DateTime>();
        }

        protected override Task DoHandleAsync(GrpcContext context)
        {
            return Task.Run(() =>
            {
                context.StartMonitor();
                _logger.LogInformation(JsonSerializer.Serialize(context.Message));
            });
        }

        protected override Task DoHandleResultAsync(GrpcContext context)
        {
            return Task.Run(() =>
            {
                AddGrpcRecord(context);
            });
        }

        private void AddGrpcRecord(GrpcContext context)
        {
            context.StopMonitor();
            var loggerData = new ArrayList();
            loggerData.Add(context.CallContext.Method);
            loggerData.Add($"{context.PerformanceInfo.UsedTime} ms");
            loggerData.Add(context.CallContext.Peer);
            loggerData.Add(context.Message);
            loggerData.Add(context.Result);

            var msg = new
            {
                Name = "GrpcStatistics",
                Interface = context.CallContext.Method,
                Spend = $"{context.PerformanceInfo.UsedTime} ms",
                FromIP = context.CallContext.Peer,
                Request = context.Message,
                Response = context.Result
            };
            _logger.LogInformation(JsonSerializer.Serialize(msg));
        }

    }
}
