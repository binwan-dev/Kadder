using System;
using System.Threading.Tasks;
using Grpc.Core;
using Kadder.Messaging;
using Kadder.Utilies;
using Microsoft.Extensions.DependencyInjection;

namespace Kadder.Middlewares
{
    public class GrpcContext
    {
        public GrpcContext(IMessageEnvelope message, ServerCallContext callContext)
        {
            Message = message ?? throw new ArgumentNullException("The message cannot be null!");
            CallContext = callContext ?? throw new ArgumentNullException("The message call context cannot be null!");
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }

        public IMessageEnvelope Message { get; }

        public IMessageResultEnvelope Result { get; set; }

        internal Func<IMessageEnvelope, IServiceScope, Task<IMessageResultEnvelope>> Hander { get; set; }

        public ServerCallContext CallContext { get; }

        [Obsolete("Please use 'Complete' method")]
        public bool HasDone
        {
            get => IsDone;
            set => IsDone = value;
        }

        public bool IsDone { get; private set; }

        public GrpcMessagePerformance PerformanceInfo { get; private set; }

        public void StartMonitor()
        {
            PerformanceInfo = new GrpcMessagePerformance(DateTime.Now);
        }

        public void StopMonitor()
        {
            if (PerformanceInfo == null)
            {
                PerformanceInfo = new GrpcMessagePerformance(DateTime.Now.AddDays(1));
            }
            PerformanceInfo.CalcUsedTime(DateTime.Now);
        }

        public void Complete()
        {
            IsDone = true;
        }
    }

    public class GrpcMessagePerformance
    {
        public GrpcMessagePerformance(DateTime startTime)
        {
            StartTime = startTime;
        }

        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }

        /// <summary>
        /// Used time, unit: ms
        /// </summary>
        public long UsedTime { get; private set; }

        public long CalcUsedTime(DateTime endTime)
        {
            EndTime = endTime;
            var timespan = EndTime - StartTime;
            UsedTime = (long)timespan.TotalMilliseconds;
            return UsedTime;
        }
    }
}
