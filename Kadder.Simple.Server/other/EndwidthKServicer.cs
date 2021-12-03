using System;
using System.Threading.Tasks;

namespace Kadder.Simple.Server
{
    public class EndwidthKServicer
    {	
        public Task HelloAsync()
        {
            Console.WriteLine("Hello, endwidth");
            return Task.CompletedTask;
        }
    }   
}
