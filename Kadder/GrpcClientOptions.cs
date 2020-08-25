namespace Kadder
{
    public class GrpcClientOptions : GrpcOptions
    {
        public GrpcClientOptions()
        {
            ConnectSecondTimeout = 10;
            AutoConnect = true;
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

        public string Strategy { get; set; }

        public bool AutoConnect { get; set; }

    }
}
