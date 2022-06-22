namespace Kadder.WebServer.Http
{
    public class HttpContext
    {
        public HttpContext(Request request, Response response)
        {
            Request = request;
            Response = response;
        }

        public Request Request { get; set; }

        public Response Response { get; set; }
    }
}