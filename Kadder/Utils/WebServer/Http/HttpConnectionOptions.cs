namespace Kadder.Utils.WebServer.Http
{
    public class HttpConnectionOptions
    {
        public HttpConnectionOptions()
        {
            KeepLiveTimeout = 65 * 1000;
        }

        public int KeepLiveTimeout{ get; set; }
    }
}
