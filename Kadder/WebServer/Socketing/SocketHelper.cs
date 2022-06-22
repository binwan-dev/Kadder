using System;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Kadder.WebServer.Socketing
{
    public class SocketHelper
    {
        public static void ShutdownSocket(Socket socket, SocketError error, string message, ILogger log, Exception ex = null)
        {
            if (error != SocketError.Success || ex != null)
            {
                log.LogError(ex, message);
            }

            var remote = socket.RemoteEndPoint.ToString();

            try
            {
                if (socket.Connected)
                    socket.Disconnect(true);
                socket.Close();
                socket.Dispose();
                log.LogInformation($"the socket({remote}) has been closed!");
            }
            catch (Exception closeEx)
            {
                log.LogError(closeEx, $"the socket({remote}) close failed!");
            }
        }
    }
}