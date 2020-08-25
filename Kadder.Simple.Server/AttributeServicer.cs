using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kadder.Simple.Server
{
    [KServicer]
    public class AttributeServicer
    {   
        public virtual Task HelloAsync()
        {
            Console.WriteLine("Hello, attribute");
            return Task.CompletedTask;
        }
    }
}
