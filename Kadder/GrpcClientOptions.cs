namespace Kadder
{
    public class GrpcClientOptions : GrpcOptions
    {
        public GrpcClientOptions()
        {
            ConnectSecondTimeout = 10;
            KeepLive = true;
        }

        public GrpcClientOptions(GrpcOptions options) : this()
        {
            Host = options.Host;
            Port = options.Port;
            NamespaceName = options.NamespaceName;
            ServiceName = options.ServiceName;
            ScanAssemblies = options.ScanAssemblies;
        }

        /// <summary>
        /// Connection timeout (unit: s)
        /// </summary>
        public int ConnectSecondTimeout { get; set; }

        /// <summary>
        /// Keep connect live
        /// </summary>
        public bool KeepLive { get; set; }

        public string Strategy { get; set; }
        
    }
}
