using System;

namespace Atlantis.Grpc.Logging.Providers
{
    public class NullLoggerProvider:ILoggerProvider
    {
        public void Config(ProviderSetting setting)
        {
            return ;
        }

        public ILogger CreateLogger(string name)
        {
            return new NullLogger();
        }
    }
}
