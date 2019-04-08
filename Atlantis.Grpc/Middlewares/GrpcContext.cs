using System;
using Followme.AspNet.Core.FastCommon.Infrastructure;
using Grpc.Core;

namespace Followme.AspNet.Core.FastCommon.ThirdParty.GrpcServer.Middlewares
{
    public class GrpcContext
    {
        public GrpcContext(BaseMessage message,ServerCallContext callContext)
        {
            Message=message??throw new ArgumentNullException("The message cannot be null!");
            CallContext=callContext?? throw new ArgumentNullException("The message call context cannot be null!");
            Id=Guid.NewGuid();
        }

        public Guid Id{get;}
        
        public BaseMessage Message{get;}

        public MessageResult Result{get;set;}

        public ServerCallContext CallContext{get;}

        public bool HasDone{get;set;}

        public GrpcMessagePerformance PerformanceInfo{get;private set;}

        public void StartMonitor()
        {
            PerformanceInfo=new GrpcMessagePerformance(DateTime.Now);
        }

        public void StopMonitor()
        {
            if(PerformanceInfo==null)PerformanceInfo=new GrpcMessagePerformance(DateTime.Now.AddDays(1));
            PerformanceInfo.CalcUsedTime(DateTime.Now);
        }
    }

    public class GrpcMessagePerformance
    {
        public GrpcMessagePerformance(DateTime startTime)
        {
            StartTime=startTime;
        }
        
        public DateTime StartTime{get;private set;}

        public DateTime EndTime{get;private set;}

        /// <summary>
        /// Used time, unit: ms
        /// </summary>
        public long UsedTime{get;private set;}
        
        public long CalcUsedTime(DateTime endTime)
        {
            EndTime=endTime;
            var timespan=EndTime -StartTime;
            UsedTime=(long)timespan.TotalMilliseconds;
            return UsedTime;
        }
    }
}
