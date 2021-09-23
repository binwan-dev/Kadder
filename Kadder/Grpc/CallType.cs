namespace Kadder.Grpc
{
    public enum CallType
    {
        Rpc = 0,
        ClientStreamRpc = 1,
        ServerStreamRpc = 2,
        DuplexStreamRpc = 3
    }
}
