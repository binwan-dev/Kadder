using System;
using System.Threading.Tasks;

namespace Kadder.Simple.Server
{
    public class JsonMessageKServicer
    {
        public Task<JsonMessageResult> HelloAsync(JsonMessage message)
        {
            return Task.FromResult(new JsonMessageResult()
            {
                HelloContent=$"Hello {message.Name}, you has {message.Age} years old"
            });
        }
    }

    public class JsonMessage
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }

    public class JsonMessageResult
    {
        public string HelloContent { get; set; }
    }
}
