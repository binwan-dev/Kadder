using System.Threading.Tasks;
using Grpc.Core;

namespace Kadder.Grpc.Client
{
    public class ServiceInvoker
    {
        public Task<TResponse> RpcCallAsync<TRequest,TResponse>(TRequest request,string service,string method)
        {
            var method=new Method<TRequest, TResponse>(
                MethodType.Unary,
                service,
                method,
                Marshaller<TRequest> requestMarshaller, Marshaller<TResponse> responseMarshaller))
        }
    }
}
