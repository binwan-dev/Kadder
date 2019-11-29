using System.Threading.Tasks;
using Kadder.Utilies;
using ProtoBuf;

namespace Kadder.Simple.Server
{
    public interface IPersonMessageServicer:IMessagingServicer
    {
        Task HelloAsync();
    }

    public class PersonMessageServicer : IPersonMessageServicer
    {
        public Task HelloAsync()
        {
            // var result = $"Hello, {message.Name}";
            return Task.CompletedTask;
        }
    }

    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    public class HelloMessage : BaseMessage
    {
        public string Name { get; set; }
        public string Nams { get; set; }
        public string Age { get; set; }
        public string Sex { get; set; }
        public string Nad { get; set; }
    }

    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    public class HelloMessageResult :MessageResult
    {
        public string Result { get; set; }
    }
}
