using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Kadder.Grpc.Client.Options;
using Kadder.Utils;

namespace Kadder.Grpc.Client
{
    public class GrpcProxyer
    {
        private static IDictionary<string, GrpcProxyer> _proxyerDict;

        static GrpcProxyer()
        {
            _proxyerDict = new Dictionary<string, GrpcProxyer>();
        }

        private readonly List<Type> _servicerTypes;
        private readonly GrpcProxyerOptions _proxyerOptions;
        private readonly IDictionary<string, ChannelInfo> _channels;

        public GrpcProxyer(List<Type> servicerTypes, GrpcProxyerOptions options)
        {
            _servicerTypes = servicerTypes;
            _proxyerOptions = options;
            _channels = new Dictionary<string, ChannelInfo>();

            foreach (var opt in options.Addresses)
                AddChannel(opt);

            foreach (var servicerType in servicerTypes)
            {
                var servicerName = servicerType.FullName;
                if (!string.IsNullOrWhiteSpace(options.PackageName))
                    servicerName = $"{options.PackageName}.{servicerType.Name}";
                ProxyerDict.Add(servicerName, this);
            }
        }

        public static IDictionary<string, GrpcProxyer> ProxyerDict => _proxyerDict;

        public IReadOnlyList<Type> ServicerTypes => _servicerTypes;

        public GrpcProxyerOptions Options => _proxyerOptions;

        public virtual ChannelInfo GetChannel()
            => _channels.FirstOrDefault().Value;

        public void AddChannel(GrpcChannelOptions options)
            => _channels.Add(options.Address, setChannels(options));


        public async Task RemoveChannelAsync(string address)
        {
            if (!_channels.TryGetValue(address, out ChannelInfo channel))
                return;

            await channel.Channel.ShutdownAsync();
            channel.Channel = null;
        }

        private ChannelInfo setChannels(GrpcChannelOptions options)
        {
            var channel = new ChannelInfo()
            {
                Channel = new Channel(options.Address, options.Credentials),
                Options = options,
                ProxyerOptions = _proxyerOptions
            };
            return channel;
        }

        public class ChannelInfo
        {
            private CallInvoker _invoker;

            public Channel Channel { get; set; }

            public GrpcProxyerOptions ProxyerOptions { get; set; }

            public GrpcChannelOptions Options { get; set; }

            public CallInvoker GetInvoker(IObjectProvider provider)
            {
                if (_invoker == null)
                {
                    _invoker = Channel.CreateCallInvoker();
                    foreach (var interceptor in ProxyerOptions.Interceptors)
                        _invoker = _invoker.Intercept((Interceptor)provider.GetObject(interceptor));
                }
                return _invoker;
            }
        }
    }
}