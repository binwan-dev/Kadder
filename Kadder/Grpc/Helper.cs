using System;
using Kadder.Streaming;

namespace Kadder.Grpc
{
    public class Helper
    {
        public static CallType AnalyseCallType(Type parameterType, Type returnParameterType)
        {
            if (parameterType == typeof(IAsyncRequestStream<>) && returnParameterType == typeof(IAsyncResponseStream<>))
                return CallType.DuplexStreamRpc;
            else if (parameterType == typeof(IAsyncRequestStream<>))
                return CallType.ClientStreamRpc;
            else if (returnParameterType == typeof(IAsyncResponseStream<>))
                return CallType.ServerStreamRpc;
            else
                return CallType.Rpc;
        }
    }
}
