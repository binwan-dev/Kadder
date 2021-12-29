/*
 * @Description: Grpc client builder class
 * @Author: Bin Wan
 * @Email: email@wanbin.tech
 */
using System.Reflection;
using System.Linq;
using System;
using System.Collections.Generic;
using Kadder.Grpc.Client.Options;
using Kadder.Utils;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kadder.Grpc.Client
{
    public class ClientBuilder : KadderBuilder
    {
        /// <summary>
        /// Default configuration key in appsetting.json file
        /// </summary>
        public const string ConfigurationKeyName = "GrpcClient";

        public ClientBuilder()
        {
            ProxyerOptions = new List<GrpcProxyerOptions>();
            GlobalInterceptors = new List<Type>();
        }

        public List<Type> GlobalInterceptors { get; internal set; }

        public List<GrpcProxyerOptions> ProxyerOptions { get; set; }

        public IConfiguration Configuration { get; internal set; }

        public IServiceCollection Services { get; internal set; }

        /// <summary>
        /// Add new grpc client for servicers.
        /// </summary>
        /// <param name="options">grpc client options</param>
        /// <returns></returns>
        public ClientBuilder AddProxyer(GrpcProxyerOptions options)
        {
            ProxyerOptions.Add(options);

            return this;
        }

        internal Client Build()
        {
            var proxyers = new List<GrpcProxyer>();

            foreach (var proxyerOptions in ProxyerOptions)
            {
                foreach (var assemblyName in proxyerOptions.AssemblyNames)
                    proxyerOptions.AddAssembly(Assembly.Load(assemblyName));
                proxyerOptions.Interceptors.AddRange(GlobalInterceptors);

                var servicerType = ServicerHelper.GetServicerTypes(proxyerOptions.Assemblies);
                proxyers.Add(new GrpcProxyer(servicerType, proxyerOptions));
                Assemblies.AddRange(proxyerOptions.Assemblies);
            }
            return new Client(proxyers, ProxyerOptions);
        }

        public ClientBuilder AddGlobalInterceptor<TInterceptor>() where TInterceptor : Interceptor
        {
            GlobalInterceptors.Add(typeof(TInterceptor));
            return this;
        }
    }
}
