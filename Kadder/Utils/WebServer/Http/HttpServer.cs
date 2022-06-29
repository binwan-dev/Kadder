using System.Threading.Tasks;
using Kadder.Utils.WebServer.Socketing;
using Microsoft.Extensions.Logging;

namespace Kadder.Utils.WebServer.Http
{
    public class HttpServer : ServerSocket
    {
        private readonly HttpServerOptions _options;

        public HttpServer(HttpServerOptions options, ILogger<HttpServer> log) : base(options.Address, options.Port, log, options.ListenPendingConns)
        {
            _options = options;
        }

        public void Start()
        {
            Bind();
            Listen();
            var _ = accept();
        }

        private async Task accept()
        {
            while (true)
            {
                var args = await AcceptAsync();
                var connection = HttpConnectionPool.Instance.GetOrCreateConnection(args.AcceptSocket, _options.ConnectionOptions);
                var _ = connection.DoReceiveAsync();
            }
        }
    }
}
