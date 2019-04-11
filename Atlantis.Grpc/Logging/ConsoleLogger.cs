using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlantis.Grpc.Logging
{
    public class ConsoleLogger : Logger
    {
        public ConsoleLogger(string name) : base(name)
        {
        }

        public override bool IsDebugEnabled => true;

        public override void Debug(string msg, Exception exception = null, object[] parameters = null)
        {
            Console.ForegroundColor=ConsoleColor.White;
            ToConsole("Debug", msg,  exception);
        }

        public override void Error(string msg, Exception exception = null, object[] parameters = null)
        {
            Console.ForegroundColor=ConsoleColor.Red;
            ToConsole("Error", msg,  exception);
        }

        public override void Fatal(string msg, Exception exception = null, object[] parameters = null)
        {
            Console.ForegroundColor=ConsoleColor.Red;
            ToConsole("Fatal", msg,  exception);
        }

        public override void Info(string msg, Exception exception = null, object[] parameters = null)
        {
            Console.ForegroundColor=ConsoleColor.White;
            ToConsole("Info", msg,  exception);
        }

        public override void Trace(string msg, Exception exception = null, object[] parameters = null)
        {
            Console.ForegroundColor=ConsoleColor.White;
            ToConsole("Trace", msg,  exception);
        }

        public override void Warn(string msg, Exception exception = null, object[] parameters = null)
        {
            Console.ForegroundColor=ConsoleColor.Yellow;
            ToConsole("Warning", msg,  exception);
        }

        private void ToConsole(string level,string msg,Exception exception)
        {
            if(exception==null)
            {
                Console.WriteLine($"Location[{_name}] Msg[{msg}]");
            }
            else
            {
                Console.WriteLine($"Location[{_name}] Msg[{msg}] Error[{exception.Message}] StackTrace[{exception.StackTrace}]");
            }
        }
    }
}
