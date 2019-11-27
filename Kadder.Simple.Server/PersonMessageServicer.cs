using System.Threading.Tasks;
using Kadder.Utilies;
using ProtoBuf;

namespace Kadder.Simple.Server
{
    public interface IPersonMessageServicer:IMessagingServicer
    {
        Task HelloAsync(HelloMessage message);
    }

    public class PersonMessageServicer : IPersonMessageServicer
    {
        public Task HelloAsync(HelloMessage message)
        {
            var result = $"Hello, {message.Name}";
            return Task.CompletedTask;
        }
    }

    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    public class HelloMessage : BaseMessage
    {
        public string Name { get; set; }
    }

    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    public class HelloMessageResult :MessageResult
    {
        public string Result { get; set; }
    }
}
