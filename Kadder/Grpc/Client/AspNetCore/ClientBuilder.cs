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

namespace Kadder.Grpc.Client.AspNetCore
{
    public class ClientBuilder : KadderBuilder
    {
        /// <summary>
        /// Default configuration key in appsetting.json file
        /// </summary>
        public const string ConfigurationKeyName = "GrpcClient";

        public ClientBuilder()
        {
            Clients = new List<GrpcClient>();
        }

        /// <summary>
        /// GrpcClient list
        /// </summary>
        /// <value></value>
        public List<GrpcClient> Clients { get; set; }

        /// <summary>
        /// Grpc servicer list
        /// </summary>
        /// <value></value>
        internal List<Type> ServicerTypes { get; set; }

        /// <summary>
        /// Add new grpc client for servicers.
        /// </summary>
        /// <param name="options">grpc client options</param>
        /// <returns></returns>
        public ClientBuilder AddClient(GrpcClientOptions options)
        {
            foreach (var assemblyName in options.AssemblyNames)
                options.Assemblies.Add(Assembly.LoadFrom(assemblyName));

            var servicerTypes = ServicerHelper.GetServicerTypes(options.Assemblies);
            new GrpcClient(servicerTypes, options);
            Assemblies.AddRange(options.Assemblies);
            ServicerTypes.AddRange(servicerTypes);
            return this;
        }
    }
}
