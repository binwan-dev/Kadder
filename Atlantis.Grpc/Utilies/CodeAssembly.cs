using System.Reflection;

namespace Atlantis.Grpc.Utilies
{
    public class CodeAssembly
    {
        private readonly Assembly _assembly;

        internal CodeAssembly(Assembly assembly)
        {
            _assembly = assembly;
        }

        public Assembly Assembly => _assembly;
        
    }
}
