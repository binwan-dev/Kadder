using System;

namespace Atlantis.Grpc.Logging.Providers
{
    public interface ILoggerProvider
    {
        void Config(ProviderSetting setting);

        ILogger CreateLogger(string name);
    }
}
