using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Kadder.Utils
{
    public class Channel<T> where T : class
    {
        private ConcurrentQueue<T> _queue;

        public Channel()
        {
            _queue = new ConcurrentQueue<T>();
        }

        public Task<T> ReadAsync()
        {
	    
	}

        public void Write(T t)
        {
            _queue.Enqueue(t);
        }
    }
}
