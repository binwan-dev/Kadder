using System;
using System.Reflection;
using Atlantis.Grpc.Logging;
using Atlantis.Grpc.Utilies;

namespace Atlantis.Grpc.Simple.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Environment.CurrentDirectory);
            var options=new GrpcServerOptions()
            {
                Host="127.0.0.1",
                Port=3002,
                NamespaceName="Atlantis.Simple",
                PackageName="Atlantis.Simple",
                ServiceName="AtlantisService",
                ScanAssemblies=new string[]
                {
                    typeof(Program).Assembly.FullName
                }
            };
            //GrpcConfiguration.LoggerFactory=new Loggerfac();

            var server=new GrpcServer(options);
            ObjectContainer.Register<IPersonMessageServicer,PersonMessageServicer>(LifeScope.Single);
            server.Start();

            Console.WriteLine("Server is running...");
            Console.ReadLine();
        }
    }

    class Loggerfac : ILoggerFactory
    {
        public ILogger Create<T>()
        {
           return new Logger();
        }

        public ILogger Create(string name)
        {
           return new Logger();
        }

        public ILogger Create(Type type)
        {
           return new Logger();
        }
    }

    class Logger : ILogger
    {
        public bool IsDebugEnabled => throw new NotImplementedException();

        public void Debug(string msg, Exception exception = null, object[] parameters = null)
        {
            throw new NotImplementedException();
        }

        public void Error(string msg, Exception exception = null, object[] parameters = null)
        {
            throw new NotImplementedException();
        }

        public void Fatal(string msg, Exception exception = null, object[] parameters = null)
        {
            throw new NotImplementedException();
        }

        public void Info(string msg, Exception exception = null, object[] parameters = null)
        {
            throw new NotImplementedException();
        }

        public void Trace(string msg, Exception exception = null, object[] parameters = null)
        {
            throw new NotImplementedException();
        }

        public void Warn(string msg, Exception exception = null, object[] parameters = null)
        {
            throw new NotImplementedException();
        }
    }
}
