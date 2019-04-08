using System.Threading.Tasks;
using Atlantis.Grpc.Utilies;

namespace Atlantis.Grpc.Simple.Server
{
    public interface IPersonMessageServicer:IMessagingServicer
    {
        Task<HelloMessageResult> HelloAsync(HelloMessage message);
    }

    public class PersonMessageServicer : IPersonMessageServicer
    {
        public Task<HelloMessageResult> HelloAsync(HelloMessage message)
        {
            var result = $"Hello, {message.Name}";
            return Task.FromResult(new HelloMessageResult()
            {
                Result = result
            });
        }
    }

    public class HelloMessage : BaseMessage
    {
        public string Name { get; set; }
    }

    public class HelloMessageResult : GrpcMessageResult
    {
        public string Result { get; set; }
    }
}
