using System;
using System.Collections.Generic;
using Followme.AspNet.Core.FastCommon.Components;
using Followme.AspNet.Core.FastCommon.Logging;
using Followme.AspNet.Core.FastCommon.Serializing;
using System.Collections;
using System.Threading.Tasks;

namespace Followme.AspNet.Core.FastCommon.ThirdParty.GrpcServer.Middlewares
{
    public class LoggerMiddleware:GrpcMiddlewareBase
    {
        private readonly ILogger _grpcLogger;
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IDictionary<Guid,DateTime> _startCallTimerDic;
        
        public LoggerMiddleware(HandlerDelegateAsync next):base(next)
        {
            _grpcLogger=ObjectContainer.Resolve<IGrpcLoggerFactory>().Create("GrpcLoggerMiddleware");
            _logger=ObjectContainer.Resolve<ILoggerFactory>().Create("GrpcLoggerMiddleware");
            _jsonSerializer=ObjectContainer.Resolve<IJsonSerializer>();
            _startCallTimerDic=new Dictionary<Guid,DateTime >();
        }

        protected override Task DoHandleAsync(GrpcContext context)
        {
            return Task.Run(() =>
            {
                context.StartMonitor();
                _logger.Info($"Receive the msg, msg type is: {context.Message.GetType().FullName}, data: {_jsonSerializer.Serialize(context.Message)}");

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
            _grpcLogger.Info("msg: "+context.Result.Message+", template: The msg({@interface_name}) used time: {@elapsed_time} ms, result status: {@status}, source ip: {@source_ip}, request data: {@request_content}, response data: {@response_content}.",null,loggerData.ToArray());
        }

    }
}
