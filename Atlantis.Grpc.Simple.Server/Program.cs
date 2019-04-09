using System;
using System.Reflection;
using Atlantis.Grpc.Utilies;

namespace Atlantis.Grpc.Simple.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var options=new GrpcServerOptions()
            {
                Host="127.0.0.1",
                Port=3002,
                NamespaceName="Atlantis.Simple",
                PackageName="Atlantis.Simple",
                ServiceName="AtlantisService",
                ScanAssemblies=new Assembly[]
                {
                    typeof(Program).Assembly
                }
            };

            var server=new GrpcServer(options);
            ObjectContainer.Register<IPersonMessageServicer,PersonMessageServicer>(LifeScope.Single);


            server.Start();

            Console.WriteLine("Server is running...");
            Console.ReadLine();
        }
    }
}
