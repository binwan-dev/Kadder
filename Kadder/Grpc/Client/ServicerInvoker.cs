using System;
using System.Threading.Tasks;
using Grpc.Core;
using Kadder.Streaming;

namespace Kadder.Grpc.Client
{
    public class ServicerInvoker
    {
        
        public Task<TResponse> RpcAsync<TRequest,TResponse>(TRequest request,string service,string method)
        {
            throw new NotImplementedException();
        }

        public IAsyncResponseStream<TResponse> ServerStreamAsync<TRequest,TResponse>(TRequest request,string service,string method)where TResponse:class
        {
            throw new NotImplementedException();
        }
    }
}
