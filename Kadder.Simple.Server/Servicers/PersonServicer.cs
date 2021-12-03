using System;
using System.Threading.Tasks;
using Kadder.Simple.Protocol.Requests;
using Kadder.Simple.Protocol.Responses;
using Kadder.Simple.Protocol.Servicers;

namespace Kadder.Simple.Server.Servicers
{
    public class PersonServicer : IPersonServicer
    {
        public Task<HelloResponse> HelloAsync(HelloRequest request)
        {
            Console.WriteLine(request.Msg);
            return Task.FromResult(new HelloResponse() { Msg = "Hello, I'm server!" });
        }
    }
}