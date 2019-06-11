using System;
using System.Collections.Generic;
using Kadder.Utilies;
using Microsoft.Extensions.Options;

namespace Kadder
{
    public class GrpcClientBuilder
    {
        private readonly IList<GrpcOptions> _clientOptions;

        public GrpcClientBuilder()
        {
            _clientOptions = new List<GrpcOptions>();
        }

        public static IServiceProvider ServiceProvider{get;set;}

        public IBinarySerializer BinarySerializer { get; set; }

        internal IList<GrpcOptions> ClientOptions=>_clientOptions;

        public GrpcClientBuilder RegClient(GrpcOptions options)
        {
            _clientOptions.Add(options);
            return this;
        }
    }
}
