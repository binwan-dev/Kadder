using System.Threading.Tasks;

namespace Kadder.Streaming
{
    public interface IAsyncResponseStream<T> : IAsyncStream where T : class
    {
    }
}
