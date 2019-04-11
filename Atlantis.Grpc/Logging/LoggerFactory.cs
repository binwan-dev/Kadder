using System;
using Atlantis.Grpc.Logging.Providers;

namespace Atlantis.Grpc.Logging
{
    public class LoggerFactory:ILoggerFactory
    {
        // private readonly ILoggerProvider _loggerProvider;

        public LoggerFactory()
        {
            // _loggerProvider=loggerProvider;
            // _loggerProvider.Config(GetSetting());
        }

        // protected ILoggerProvider Provider=>_loggerProvider;
        
        public virtual ILogger Create<T>()
        {
            return GrpcConfiguration.LoggerFunc(typeof(T));
        }
        
        public virtual ILogger Create(Type type)
        {
            return GrpcConfiguration.LoggerFunc(type);
        }

        // protected abstract ProviderSetting GetSetting();
    }
}
