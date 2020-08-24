using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GenAssembly;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Kadder.Utilies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kadder
{
    public class GrpcConnection
    {
        private readonly GrpcClientOptions _options;
        private CallInvoker _grpcInvoker;
        private readonly GrpcClientMetadata _metadata;
        private Channel _channel;
        private ILogger<GrpcClient> _log;
        private DateTime _lastConnectedTime;
        private int _connecting = 0;
        private bool _isConnected = false;
        private readonly RpcHost _host;
        private readonly Guid _id;

        public GrpcConnection(GrpcClientMetadata metadata, RpcHost host)
        {
            _metadata = metadata;
            _options = metadata.Options;
            _lastConnectedTime = DateTime.Now;
            _host = host;
            _id = Guid.NewGuid();
        }

        public Guid ID => _id;

        public RpcHost Host => _host;

        protected ILogger<GrpcClient> Log
        {
            get
            {
                if (_log == null)
                {
                    _log = GrpcClientBuilder.ServiceProvider.GetService<ILogger<GrpcClient>>();
                }
                return _log;
            }
        }

        public async Task<CallInvoker> GetInvokerAsync()
        {
            if (_grpcInvoker == null)
            {
                await CreateChannelAsync();

                _grpcInvoker = new DefaultCallInvoker(_channel);
                foreach (var interceptorType in _metadata.PublicInterceptors)
                {
                    var interceptor = (Interceptor)GrpcClientBuilder.ServiceProvider.GetService(interceptorType);
                    _grpcInvoker = _grpcInvoker.Intercept(interceptor);
                }
                foreach (var interceptorType in _metadata.PrivateInterceptors)
                {
                    var interceptor = (Interceptor)GrpcClientBuilder.ServiceProvider.GetService(interceptorType);
                    _grpcInvoker = _grpcInvoker.Intercept(interceptor);
                }
            }
            return _grpcInvoker;
        }

        protected async virtual Task<Channel> CreateChannelAsync()
        {
            while (!(Interlocked.CompareExchange(ref _connecting, 1, 0) == 0))
            {
                System.Threading.Thread.Sleep(500);
                if (_isConnected)
                {
                    return _channel;
                }
            }

            try
            {
                _channel = new Channel(_host.Host, _host.Port, ChannelCredentials.Insecure);
                await _channel.ConnectAsync(DateTime.UtcNow.AddSeconds(_metadata.Options.ConnectSecondTimeout));
                _isConnected = true;
                return _channel;
            }
            catch (TaskCanceledException)
            {
                throw new RpcException(new Status(StatusCode.Aborted, ""), $"Faild connect {_channel.Target}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Interlocked.Exchange(ref _connecting, 0);
            }
        }

    }
}
