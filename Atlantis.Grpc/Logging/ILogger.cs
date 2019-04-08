using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlantis.Grpc.Logging
{
    public interface ILogger
    {
        bool IsDebugEnabled { get; }

        void Trace(string msg, Exception exception = null,object[] parameters=null);

        void Debug(string msg, Exception exception = null,object[] parameters=null);

        void Info(string msg, Exception exception = null,object[] parameters=null);

        void Warn(string msg, Exception exception = null,object[] parameters=null);

        void Error(string msg, Exception exception = null,object[] parameters=null);

        void Fatal(string msg, Exception exception = null,object[] parameters=null);
    }
}
