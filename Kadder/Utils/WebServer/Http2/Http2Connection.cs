using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Kadder.Utils.WebServer.Socketing;
using Kadder.Utils.WebServer.Http;

namespace Kadder.Utils.WebServer.Http2
{
    public class Http2Connection:TcpConnection,IDisposable
    {
        private bool _isDisposed;
        private bool _isHttp2;
        private byte[] _connectionPerface;
        private BinaryWriter _receiveChannel;
        private Channel<byte[]> _sendChannel;
        private readonly FrameHandler _frameHandler;

        public Http2Connection(Socket socket, ConnectionSetting setting, FrameHandler frameHandler) : base(socket)
        {
	    ConnectionSetting = setting;
	    ClientConnectionSetting = setting.Clone();
            _frameHandler = frameHandler;
            WindowSize = setting.InitialWindowSize;
            ClientWindowSize = setting.InitialWindowSize;
            _connectionPerface = new byte[24] { 80, 82, 73, 32, 42, 32, 72, 84, 84, 80, 47, 50, 46, 48, 13, 10, 13, 10, 83, 77, 13, 10, 13, 10 };
            _sendChannel = Channel.CreateUnbounded<byte[]>();
            _receiveChannel = Channel.CreateUnbounded<ArraySegment<byte>>();

            Task.Run(sendHandler);
            Task.Run(receiveHandler);
        }

	public ConnectionSetting ConnectionSetting{ get; internal set; }

        public ConnectionSetting ClientConnectionSetting{ get;internal set; }

	public int WindowSize{ get; internal set; }

	public int ClientWindowSize{ get; internal set; }

        public async Task QueueSendDataAsync(byte[] data)
        {
            await _sendChannel.Writer.WriteAsync(data);
        }

        public Task SendDataAsync(byte[] data) => _socket.SendAsync(data, SocketFlags.None);

        private async Task sendHandler()
        {
            while (true)
            {
		if(_isDisposed)
                    return;

                var buffer = await _sendChannel.Reader.ReadAsync();
                await _socket.SendAsync(buffer, SocketFlags.None);
            }
        }

        private async Task receiveHandler()
        {
            while (true)
            {
		if(_isDisposed)
                    return;
		
                var buffer = await _receiveChannel.Reader.ReadAsync();
                _frameHandler.Handle(this,buffer);
                BufferPool.Instance.ArrayPool.Return(buffer.Array);
            }
        }

        private async Task sendSettingFrameAck()
        {
            var _ackBuffer = new byte[] { 0, 0, 0, 4, 1, 0, 0, 0, 0 };
	    await _socket.SendAsync(_ackBuffer,SocketFlags.None);
        }

        public override async Task DoReceiveAsync()
        {
            var prevBuffer = new ArraySegment<byte>();
            while (true)
            {
                var buffer = BufferPool.Instance.ArrayPool.Rent(1024 * 1024 * 2);

                var offest = await receiveAsync(buffer);
                if (offest == 0)
                    break;

                var bufferArr = new ArraySegment<byte>(buffer, 0, offest);
                if (!_isHttp2)
                {
                    _isHttp2 = isConnectionPerface(bufferArr);
                    if (!_isHttp2)
                        break;
                    bufferArr = bufferArr.Slice(_connectionPerface.Length);
                }

                while (bufferArr.Count > 0)
                {
                    var length = 0;
                    if (prevBuffer.Count > 0)
                    {
                        if (prevBuffer.Count >= 3)
                        {
                            length = (int) ((prevBuffer[0] & 16) | ((prevBuffer[1] & 0xFF) << 8) |
                                            (prevBuffer[2] & 0xFF));
                            prevBuffer = prevBuffer.Slice(0, 3);
                        }
                        else
                        {
                            var tempBuffer = new byte[3];
                            prevBuffer.CopyTo(tempBuffer);
                            var needBufferLength = 3 - prevBuffer.Count;
                            bufferArr.CopyTo(tempBuffer, prevBuffer.Count);
                            length = (int) ((tempBuffer[0] & 16) | ((tempBuffer[1] & 0xFF) << 8) |
                                            (tempBuffer[2] & 0xFF));
                            prevBuffer = prevBuffer.Slice(0, prevBuffer.Count);
                            bufferArr = bufferArr.Slice(0, needBufferLength);
                        }
                    }
                    else
                    {
                        length = (int) ((bufferArr[0] & 16) | ((bufferArr[1] & 0xFF) << 8) | (bufferArr[2] & 0xFF));
                    }

                    if (length == 21081 && isConnectionPerface(bufferArr))
                    {
                        bufferArr = bufferArr.Slice(_connectionPerface.Length);
                        continue;
                    }


                    if (bufferArr.Count < length)
                    {
                        prevBuffer = bufferArr;
                        break;
                    }

                    var frameBuffer = bufferArr.Slice(0, length + 9);
                    await _receiveChannel.Writer.WriteAsync(frameBuffer);
                    bufferArr = bufferArr.Slice(length + 9);
                }
            }

            var _ = Task.Run(Dispose);
        }

        private bool isConnectionPerface(ArraySegment<byte> buffer)
        {
	    if(buffer.Count<_connectionPerface.Length)
                return false;
            for (var i = 0; i < _connectionPerface.Length; i++)
            {
		if(buffer[i]!=_connectionPerface[i])
                    return false;
            }

            return true;
        }
	
	public void Dispose()
        {
	    if(_isDisposed)
                return;

            _isDisposed = true;
            _receiveSocketArgs.AcceptSocket = null;
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket.Dispose();
            _socket = null;
            HttpConnectionPool.Instance.ReturnConnection(this);
        }

    }
}
