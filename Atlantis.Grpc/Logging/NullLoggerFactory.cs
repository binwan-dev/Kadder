using System;

namespace Atlantis.Grpc.Logging
{
    public class NullLoggerFactory : ILoggerFactory
    {
        public ILogger Create<T>()
        {
            return new NullLogger();
        }

        public ILogger Create(string name)
        {
            return new NullLogger();
        }

        public ILogger Create(Type type)
        {
            return new NullLogger();
        }
    }
}
