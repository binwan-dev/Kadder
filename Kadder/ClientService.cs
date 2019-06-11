using System.Collections.Generic;

namespace Kadder
{   
    public class ClientService<T>
    {
        protected static readonly IDictionary<string,GrpcClient> _clientDic=
            new Dictionary<string,GrpcClient>();

        // public ClientService(GrpcOptions options)
        // {
        //     var id=options.
        // }
        
        public T Agent{get;}
    }
}
