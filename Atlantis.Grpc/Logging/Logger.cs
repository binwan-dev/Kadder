using System;
using Atlantis.Grpc.Logging;

namespace Atlantis.Grpc
{
    public abstract class Logger:ILogger
    {
        protected string _name;

        public Logger(string name)
        {
            _name=name;
        }

        public abstract bool IsDebugEnabled{get;}

        public abstract void Debug(string msg, Exception exception = null, object[] parameters = null);

        public abstract void Error(string msg, Exception exception = null, object[] parameters = null);

        public abstract void Fatal(string msg, Exception exception = null, object[] parameters = null);

        public abstract void Info(string msg, Exception exception = null, object[] parameters = null);

        public abstract void Trace(string msg, Exception exception = null, object[] parameters = null);

        public abstract void Warn(string msg, Exception exception = null, object[] parameters = null);
    }
}
