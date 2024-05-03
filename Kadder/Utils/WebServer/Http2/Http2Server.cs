using System;
using System.Threading.Tasks;
using Kadder.Utils.WebServer.Http;
using Kadder.Utils.WebServer.Socketing;
using Microsoft.Extensions.Logging;

namespace Kadder.Utils.WebServer.Http2
{
    public class Http2Server : ServerSocket
    {
        private readonly HttpServerOptions _options;
        private readonly FrameHandler _frameHandler;
        private readonly ConnectionSetting _connectionSetting;

        public Http2Server(HttpServerOptions options, ILogger<Http2Server> log,ConnectionSetting connectionSetting, FrameHandler frameHandler) : base(options.Address, options.Port, log, options.ListenPendingConns)
        {
            _options = options;
            _frameHandler = frameHandler;
            _connectionSetting = connectionSetting;
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
                var connection = HttpConnectionPool.Instance.GetOrCreateConnection(args.AcceptSocket, _connectionSetting, _frameHandler);
                var _ = connection.DoReceiveAsync();
            }
        }
    }
}
