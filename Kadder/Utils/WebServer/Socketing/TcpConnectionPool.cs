using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using Kadder.Utils.WebServer.Http;
using Microsoft.Extensions.Logging;

namespace Kadder.Utils.WebServer.Socketing
{
    public class TcpConnectionPool
    {
        private readonly ConcurrentQueue<TcpConnection> _freeQueue;

        public static TcpConnectionPool Instance;

        static TcpConnectionPool() => Instance = new TcpConnectionPool();

        public TcpConnectionPool()
        {
            _freeQueue = new ConcurrentQueue<TcpConnection>();
        }

        public void ReturnConnection(TcpConnection connection)
        {
            _freeQueue.Enqueue(connection);
        }

        public TcpConnection GetOrCreateConnection(Socket socket,ILogger<TcpConnection> log)
        {
            if (_freeQueue.TryDequeue(out TcpConnection connection))
            {
                connection.SetNewFromPool(socket);
                return connection;
            }

            return new HttpConnection(socket, null);
        }
    }
}
