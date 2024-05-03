using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;
using Kadder.Utils.WebServer.Http;

namespace Kadder.Utils.WebServer.Http2
{
    public class HttpConnectionPool
    {
	private readonly ConcurrentQueue<Http2Connection> _freeQueue;

        public static HttpConnectionPool Instance;

        static HttpConnectionPool() => Instance = new HttpConnectionPool();

        public HttpConnectionPool()
        {
            _freeQueue = new ConcurrentQueue<Http2Connection>();
        }

        public void ReturnConnection(Http2Connection connection)
        {
            _freeQueue.Enqueue(connection);
        }

        public Http2Connection GetOrCreateConnection(Socket socket, ConnectionSetting connectionSetting,
            FrameHandler frameHandler)
        {
            if (_freeQueue.TryDequeue(out Http2Connection connection))
            {
                connection.SetNewFromPool(socket);
                return connection;
            }

            return new Http2Connection(socket, connectionSetting, frameHandler);
        }

    }
}
