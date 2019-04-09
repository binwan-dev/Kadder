using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlantis.Grpc.Logging
{
    public class NullLogger : ILogger
    {
        public bool IsDebugEnabled => false;

        public void Debug(string msg, Exception exception = null,object[] parameters=null)
        {
            Console.WriteLine(msg);
        }

        public void Error(string msg, Exception exception = null,object[] parameters=null)
        {
            Console.WriteLine(msg);
        }

        public void Fatal(string msg, Exception exception = null,object[] parameters=null)
        {
            Console.WriteLine(msg);
        }

        public void Info(string msg, Exception exception = null,object[] parameters=null)
        {
            Console.WriteLine(msg);
        }

        public void Trace(string msg, Exception exception = null,object[] parameters=null)
        {
        }

        public void Warn(string msg, Exception exception = null,object[] parameters=null)
        {
            Console.WriteLine(msg);
        }
    }
}
