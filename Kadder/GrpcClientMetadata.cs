using Grpc.Core.Interceptors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kadder
{
    public class GrpcClientMetadata
    {
        public GrpcClientMetadata(GrpcOptions options)
        {
            Options = options;
            PrivateInterceptors = new List<Type>();
        }

        public IList<Type> PrivateInterceptors { get; set; }

        public GrpcOptions Options { get; set; }

        public GrpcClientMetadata RegInterceptor<T>() where T : Interceptor
        {
            PrivateInterceptors.Add(typeof(T));
            return this;
        }
    }
}
