using System.Threading.Tasks;
using Kadder.Utilies;
using Microsoft.Extensions.Logging;
using ProtoBuf;

namespace Kadder.Simple.Server
{
    public interface IPersonMessageServicer:IMessagingServicer
    {
        Task<HelloMessageResult> HelloAsync();

        [NotGrpcMethod]
        Task NoGrpcMethodAsync(ushort logg);
    }

    public class PersonMessageServicer : IPersonMessageServicer
    {
        public PersonMessageServicer()
        {
        }

        public Task<HelloMessageResult> HelloAsync()
        {
            // var result = $"Hello, {message.Name}";
            return Task.FromResult(new HelloMessageResult(){Result="server"});
        }

        public Task NoGrpcMethodAsync(ushort logg)
        {
            throw new System.NotImplementedException();
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
