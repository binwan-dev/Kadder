using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using Atlantis.Grpc.Logging;
using Atlantis.Grpc.Utilies;

namespace Atlantis.Grpc.Middlewares
{
    public class LoggerMiddleware:GrpcMiddlewareBase
    {
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IDictionary<Guid,DateTime> _startCallTimerDic;
        
        public LoggerMiddleware(HandlerDelegateAsync next):base(next)
        {
            _logger=ObjectContainer.Resolve<ILoggerFactory>().Create<LoggerMiddleware>();
            _jsonSerializer=ObjectContainer.Resolve<IJsonSerializer>();
            _startCallTimerDic=new Dictionary<Guid,DateTime >();
        }

        protected override Task DoHandleAsync(GrpcContext context)
        {
            return Task.Run(() =>
            {
                context.StartMonitor();
                _logger.Info(_jsonSerializer.Serialize(context.Message));
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
            var loggerData=new ArrayList();
            loggerData.Add(context.CallContext.Method);
            loggerData.Add($"{context.PerformanceInfo.UsedTime} ms");
            loggerData.Add(context.Result.Status);
            loggerData.Add(context.CallContext.Peer);
            loggerData.Add(context.Message);
            loggerData.Add(context.Result);

            var msg=new
            {
                Name="GrpcStatistics",
                Interface=context.CallContext.Method,
                Spend=$"{context.PerformanceInfo.UsedTime} ms",
                Status=context.Result.Status.ToString(),
                FromIP=context.CallContext.Peer,
                Request=context.Message,
                Response=context.Result
            };
            _logger.Info(_jsonSerializer.Serialize(msg));
        }

    }
}
