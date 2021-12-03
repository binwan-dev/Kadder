using System.Threading.Tasks;
using Kadder.Simple.Protocol.Requests;
using Kadder.Simple.Protocol.Responses;
using Kadder.Utilies;

namespace Kadder.Simple.Protocol.Servicers
{
    public interface IPersonServicer : IMessagingServicer
    {
        Task<HelloResponse> HelloAsync(HelloRequest request);
    }
}