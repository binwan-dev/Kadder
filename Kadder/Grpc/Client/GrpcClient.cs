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
    public class GrpcClient
    {
        private static IDictionary<string, GrpcClient> _clientDict;

        static GrpcClient()
        {
            _clientDict = new Dictionary<string, GrpcClient>();
        }

        private readonly List<Type> _servicerTypes;
        private readonly GrpcClientOptions _clientOptions;
        private readonly IDictionary<string, ChannelInfo> _channels;

        public GrpcClient(List<Type> servicerTypes, GrpcClientOptions options)
        {
            _servicerTypes = servicerTypes;
            _clientOptions = options;
            _channels = new Dictionary<string, ChannelInfo>();

            foreach (var opt in options.Addresses)
                AddChannel(opt);

            foreach (var servicerType in servicerTypes)
            {
                var servicerName = servicerType.FullName;
                if (!string.IsNullOrWhiteSpace(options.PackageName))
                    servicerName = $"{options.PackageName}.{servicerType.Name}";
                ClientDict.Add(servicerName, this);
            }
        }

        public static IDictionary<string, GrpcClient> ClientDict => _clientDict;

        public IReadOnlyList<Type> ServicerTypes => _servicerTypes;

        public GrpcClientOptions Options => _clientOptions;

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
                ClientOptions = _clientOptions
            };
            return channel;
        }

        public class ChannelInfo
        {
            private CallInvoker _invoker;

            public Channel Channel { get; set; }

            public GrpcClientOptions ClientOptions { get; set; }

            public GrpcChannelOptions Options { get; set; }

            public CallInvoker GetInvoker(IObjectProvider provider)
            {
                if (_invoker == null)
                {
                    _invoker = Channel.CreateCallInvoker();
                    foreach (var interceptor in ClientOptions.Interceptors)
                        _invoker = _invoker.Intercept((Interceptor)provider.GetObject(interceptor));
                }
                return _invoker;
            }
        }
    }
}