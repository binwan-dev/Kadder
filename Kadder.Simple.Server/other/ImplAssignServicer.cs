using System;
using System.Threading.Tasks;

namespace Kadder.Simple.Server
{
    public class ImplAssignServicer : ImplServicer
    {
        public Task HelloAssignAsync()
        {
            Console.WriteLine("Hello, impl");
            return Task.CompletedTask;
        }
    }
}
