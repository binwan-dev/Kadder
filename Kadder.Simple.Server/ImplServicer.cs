using System;
using System.Threading.Tasks;

namespace Kadder.Simple.Server
{
    public class ImplServicer:KServicer
    {	
    	public virtual Task HelloAsync()
        {
            Console.WriteLine("Hello, impl");
            return Task.CompletedTask;
        }
    }   
}
