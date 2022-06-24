using System.Text;
using System.Collections.Concurrent;

namespace Kadder.WebServer.Http.Pipe
{
    public class InitPipe
    {
        private readonly ConcurrentQueue<HttpContext> _contextQueue;

        public InitPipe()
        {
            _contextQueue = new ConcurrentQueue<HttpContext>();
        }

        public void Handler(HttpContext context)
        {
            _contextQueue.Enqueue(context);
            context.Response.Version = context.Request.Version;
            context.Response.StatusCode = StatusCode.OK;
            context.Response.Write(Encoding.UTF8.GetBytes("Hello world!"));
            context.Response.Flush();
        }
    }
}