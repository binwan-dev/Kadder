using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlantis.Grpc.Logging
{
    public interface ILoggerFactory
    {
        ILogger Create<T>();

        ILogger Create(string name);

        ILogger Create(Type type);
    }
}
