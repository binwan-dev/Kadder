using System;

namespace Kadder.Utilies
{
    public struct RpcHost
    {
        public RpcHost(string scheme, string host, int port)
        {
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentNullException(nameof(host));

            Scheme = string.IsNullOrWhiteSpace(scheme) ? "http" : scheme;
            Host = host;
            Port = port == 0 ? 80 : port;
        }

        public string Host { get; set; }

        public int Port { get; set; }

        public string Scheme { get; set; }

        public override string ToString()
        {
            return $"{Scheme}//{Host}:{Port}";
        }

        public static RpcHost Parse(string rpcHost)
        {
            if (string.IsNullOrWhiteSpace(rpcHost)) throw new ArgumentNullException(nameof(rpcHost));

            var scheme = "http";
            if (rpcHost.Contains("//"))
            {
                var index = rpcHost.IndexOf("//");
                scheme = rpcHost.Substring(0, index);
                rpcHost = rpcHost.Substring(index + 2);
            }

            var arr = rpcHost.Split(':');
            var host = arr[0];
            var port = arr.Length > 1 ? int.Parse(arr[1]) : 0;

            return new RpcHost(scheme, host, port);
        }
    }
}
