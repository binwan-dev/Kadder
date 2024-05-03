namespace Kadder.Utils.WebServer.Http2
{

    public class Request
    {
        public HeaderFrame HeaderFrame { get; set; }

        public DataFrame DataFrame { get; set; }
    }
}