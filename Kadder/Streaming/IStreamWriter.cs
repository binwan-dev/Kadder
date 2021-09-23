using System.Threading.Tasks;

namespace Kadder.Streaming
{
    public interface IStreamWriter<T> where T : class
    {
        Task WriteAsync(T response);
    }
}
