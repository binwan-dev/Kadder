using System.Net.Sockets;
using System.Text;

namespace Kadder.Simple.WebServer;

public class HttpClientTest
{
    public static void TestNoHeaderAndNoBody(string host, int port)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, port);
        var args = new SocketAsyncEventArgs();
        args.SetBuffer(new byte[1024 * 1024 * 5], 0, 1024 * 1024 * 5);
        args.Completed += (o, e) =>
        {
            var s = socket.Connected;
            Console.WriteLine(Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred));
        };
        socket.ReceiveAsync(args);
        socket.Send(Encoding.UTF8.GetBytes("GET / HTTP/1.1\r\n\r\n"));

    }
}