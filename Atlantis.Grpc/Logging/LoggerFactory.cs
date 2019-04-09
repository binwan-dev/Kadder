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
            return new NullLogger();
            // return _loggerProvider.CreateLogger(typeof(T).Name);
        }

        public virtual ILogger Create(string name)
        {
            return new NullLogger();
            // return _loggerProvider.CreateLogger(name);
        }

        public virtual ILogger Create(Type type)
        {
            return new NullLogger();
            // return _loggerProvider.CreateLogger(type.Name);
        }

        // protected abstract ProviderSetting GetSetting();
    }
}
