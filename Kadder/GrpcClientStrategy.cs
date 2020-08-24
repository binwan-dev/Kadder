using System;

namespace Kadder
{
    public interface IGrpcClientStrategy
    {	
    	IGrpcClientStrategy AddConn(GrpcConnection conn);

        GrpcConnection GetConn();

        IGrpcClientStrategy ConnectBroken(GrpcConnection conn);
    }   
}
