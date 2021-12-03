using System;
using System.Threading.Tasks;
using Kadder.Utilies;
using ProtoBuf;

namespace Kadder.Simple.Server
{
    public interface INumberMessageServicer : IMessagingServicer
    {
        Task PrintAsync(NumberMessage message);
    }

    public class NumberMessageServicer : INumberMessageServicer
    {
        public Task PrintAsync(NumberMessage message)
        {
            Console.WriteLine(message.Number);
            return Task.CompletedTask;
        }
    }

    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    public class NumberMessage
    {
        public int Number { get; set; }
    }
}
