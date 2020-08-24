using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Kadder
{
    public class RoundRobinStrategy : IGrpcClientStrategy
    {
        private readonly ConcurrentQueue<GrpcConnection> _connQueue;
        private readonly IList<Guid> _brokenConns;

        public RoundRobinStrategy()
        {
            _connQueue = new ConcurrentQueue<GrpcConnection>();
            _brokenConns = new List<Guid>();
        }

        public IGrpcClientStrategy AddConn(GrpcConnection conn)
        {
            if (_connQueue.Count(p => p.ID == conn.ID) > 0) return this;
            _connQueue.Enqueue(conn);
            return this;
        }

        public IGrpcClientStrategy ConnectBroken(GrpcConnection conn)
        {
            if (_brokenConns.Contains(conn.ID)) return this;
            _brokenConns.Add(conn.ID);
            return this;
        }

        public GrpcConnection GetConn()
        {
            if (!_connQueue.TryDequeue(out GrpcConnection result))
            {
                throw new IndexOutOfRangeException("No available connection");
            }
            if (!_brokenConns.Contains(result.ID))
            {
                _connQueue.Enqueue(result);
                return result;
            }
            _brokenConns.Remove(result.ID);
            return GetConn();
        }
    }
}
