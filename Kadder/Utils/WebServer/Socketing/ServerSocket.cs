using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Kadder.Utils.WebServer.Socketing
{
    public class ServerSocket
    {
        private readonly Socket _socket;
        private readonly IPEndPoint _listenEndPoint;
        private readonly SocketAwaitableEventArgs _acceptSocketArgs;
        private readonly int _listenPendingConns;
        private readonly ILogger<ServerSocket> _log;

        public ServerSocket(string listenAddress, int port, ILogger<ServerSocket> log, int listenPendingConns = -1)
        {
            _listenPendingConns = listenPendingConns;
            _log = log;
            _listenEndPoint = new IPEndPoint(IPAddress.Parse(listenAddress), port);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _acceptSocketArgs = new SocketAwaitableEventArgs();
        }

        public void Bind()
        {
            ArgumentNullException.ThrowIfNull(_listenEndPoint, nameof(_listenEndPoint));
            ArgumentNullException.ThrowIfNull(_listenEndPoint.Address, "Address");
	    
            if(_listenEndPoint.Port<=0)
                throw new InvalidCastException($"Invalid Port({_listenEndPoint.Port})");

            _socket.Bind(_listenEndPoint);
        }

        public void Listen()
        {
	    if(_listenPendingConns<=0)
                _socket.Listen();
	    else
                _socket.Listen(_listenPendingConns);
        }

        public async Task<SocketAwaitableEventArgs> AcceptAsync()
	{
	    if(_acceptSocketArgs.AcceptSocket!=null)
		_acceptSocketArgs.AcceptSocket = null;
	    
	    await acceptAsync(_acceptSocketArgs);
            return _acceptSocketArgs;
        }

        private SocketAwaitableEventArgs acceptAsync(SocketAwaitableEventArgs args)
        {
            if(!_socket.AcceptAsync(args))
                args.Complete();
            return args;
        }

    }
}
