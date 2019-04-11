using System;

namespace Atlantis.Grpc.Logging
{
    public class NullLoggerFactory : ILoggerFactory
    {
        public ILogger Create<T>()
        {
            throw new NotImplementedException();
        }

        public ILogger Create(Type type)
        {
            throw new NotImplementedException();
        }
    }
}
