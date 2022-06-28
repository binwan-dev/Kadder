using System;
using System.Buffers;

namespace Kadder.Utils.WebServer.Socketing
{
    public class BufferPool
    {
        private readonly int _maxPoolSize;
        private readonly int _changing = 0;

        public static BufferPool Instance;

        static BufferPool() => Instance = new BufferPool();

        public BufferPool()
        {
            ArrayPool = ArrayPool<byte>.Create(1024 * 1024 * 2, 1024);
        }

	public ArrayPool<byte> ArrayPool{ get;private set; }

    }
}
