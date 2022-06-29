using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Kadder.WebServer.Http;

namespace Kadder.Utils.WebServer.Http.Pipe
{
    public class InitPipe
    {
        private readonly ConcurrentQueue<HttpContext> _contextQueue;

        public InitPipe()
        {
            _contextQueue = new ConcurrentQueue<HttpContext>();
        }

        public async Task HandlerAsync(HttpContext context)
        {
            _contextQueue.Enqueue(context);
            context.Response.Version = context.Request.Version;
            context.Response.StatusCode = StatusCode.OK;
            await context.Response.WriteAsync(Encoding.UTF8.GetBytes("Hello world!"));
            await context.Response.FlushAsync();
        }
    }
}
