using System;
using System.Collections.Generic;
using System.Text;

namespace Kadder.Grpc.Server
{
    public class GrpcServerBuilder : KadderBuilder
    {
        public GrpcServerBuilder()
        {
            GrpcServicerProxyers = new List<Type>();
            Interceptors = new List<Type>();
            Options = new GrpcServerOptions();
            AssemblyNames = new List<string>();
        }

        public GrpcServerOptions Options { get; set; }
        
        public string CodeCacheDir { get; set; } = string.Empty;

        public string DllCacheDir { get; set; } = string.Empty;

        internal IList<Type> GrpcServicerProxyers { get; set; }

        internal List<Type> Interceptors { get; set; }

        public List<string> AssemblyNames { get; set; }

        public GrpcServerBuilder AddInterceptor<Interceptor>()
        {
            Interceptors.Add(typeof(Interceptor));
            return this;
        }

        public override string ToString()
        {
            var str = new StringBuilder();
            str.AppendLine("############# Grpc Server Options #############");
            str.AppendLine("Options");
            str.AppendLine($"  PackageName: {Options.PackageName}");
            str.AppendLine($"  IsGeneralProtoFile: {Options.IsGeneralProtoFile}");
            str.AppendLine("  ChannelOptions");
            foreach(var channel in Options.ChannelOptions)
                str.AppendLine($"    Name: {channel.Name}, Value: {channel.StringValue}");
            str.AppendLine("  ListenPorts");	    
	    foreach(var port in Options.Ports)
                str.AppendLine($"    Name: {port.Name}, Host: {port.Host}, Port: {port.Port}, Credentials: {port.Credentials.GetType().Name}");

            str.AppendLine();
            str.AppendLine("Assemblies:");
	    foreach(var assembly in Assemblies)
                str.AppendLine($"  {assembly.FullName}");

            str.AppendLine();
            str.AppendLine("Interceptors:");
            foreach (var interceptor in Interceptors)
                str.AppendLine($"  {interceptor.Name}");

            str.AppendLine();
            str.AppendLine("############# Grpc Server Options #############");	    

            return str.ToString();
        }
    }
}
