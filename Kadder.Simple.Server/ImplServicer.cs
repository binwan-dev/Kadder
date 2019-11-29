using System;
using System.Threading.Tasks;

namespace Kadder.Simple.Server
{
    public class ImplServicer:KServicer
    {	
    	public Task HelloAsync()
        {
            Console.WriteLine("Hello, impl");
            return Task.CompletedTask;
        }
    }   
}
