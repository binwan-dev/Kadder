using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Kadder.Utils.WebServer.Http
{
    public class HttpConnectionPool
    {
	private readonly ConcurrentQueue<HttpConnection> _freeQueue;

        public static HttpConnectionPool Instance;

        static HttpConnectionPool() => Instance = new HttpConnectionPool();

        public HttpConnectionPool()
        {
            _freeQueue = new ConcurrentQueue<HttpConnection>();
        }

        public void ReturnConnection(HttpConnection connection)
        {
            _freeQueue.Enqueue(connection);
        }

        public HttpConnection GetOrCreateConnection(Socket socket,HttpConnectionOptions options)
        {
            if (_freeQueue.TryDequeue(out HttpConnection connection))
            {
                connection.SetNewFromPool(socket);
                return connection;
            }
	    
            return new HttpConnection(socket, options);
        }

    }
}
