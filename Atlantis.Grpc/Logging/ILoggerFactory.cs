using System;

namespace Atlantis.Grpc.Logging
{
    public interface ILoggerFactory
    {
        ILogger Create<T>();

        ILogger Create(Type type);
    }
}
