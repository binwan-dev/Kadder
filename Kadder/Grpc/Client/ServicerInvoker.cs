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

        public Task<TResponse> ClientStreamAsync<TRequest,TResponse>(IAsyncRequestStream<TRequest> request, string service, string method) where TRequest:class
        {
            throw new NotImplementedException();
        }

        public Task ServerStreamAsync<TRequest,TResponse>(TRequest request, IAsyncResponseStream<TResponse> response,string service,string method)where TResponse:class
        {
            throw new NotImplementedException();
        }

        public Task DuplexStreamAsync<TRequest,TResponse>(IAsyncRequestStream<TRequest> request, IAsyncResponseStream<TResponse> response,string service,string method) where TRequest:class where TResponse:class
        {
            throw new NotImplementedException();
        }
    }
}
