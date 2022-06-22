using System;

namespace Kadder.WebServer.Socketing
{
    public class BufferPool
    {
        private readonly ArraySegment<byte> _buffers;
        private readonly int _maxPoolSize;
        private readonly int _changing = 0;

        public BufferPool()
        {
            var initSize = 1024 * 1024;
            if (initSize > _maxPoolSize)
                initSize = _maxPoolSize;

            _buffers = new ArraySegment<byte>(new byte[initSize]);
        }


    }
}