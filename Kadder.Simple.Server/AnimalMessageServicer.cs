using System.Threading.Tasks;
using Kadder.Utilies;

namespace Kadder.Simple.Server
{
    public interface IAnimalMessageServicer : IMessagingServicer
    {
        Task<HelloMessageResult> HelloAsync(HelloMessage message);
    }
    
    public class AnimalMessageServicer : IAnimalMessageServicer
    {
        public Task<HelloMessageResult> HelloAsync(HelloMessage message)
        {
            return Task.FromResult(new HelloMessageResult() { Result = "Animal" });
        }
    }
}
