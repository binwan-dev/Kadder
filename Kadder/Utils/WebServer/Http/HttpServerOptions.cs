namespace Kadder.Utils.WebServer.Http
{
    public class HttpServerOptions
    {
        public HttpServerOptions()
        {
            ListenPendingConns = -1;
            ConnectionOptions = new HttpConnectionOptions();
        }

	public HttpConnectionOptions ConnectionOptions{ get; set; }

        public string Address{ get; set; }

        public int Port{ get; set; }

        public int ListenPendingConns{ get; set; }
    }
}
