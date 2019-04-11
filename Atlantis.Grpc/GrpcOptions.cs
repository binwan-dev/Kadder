using System.Collections.Generic;
using System.Reflection;

namespace Atlantis.Grpc
{
    public class GrpcOptions
    {
        public GrpcOptions()
        {
            ScanAssemblies=new string[0];
        }
        
        public string Host{get;set;}

        public int Port{get;set;}

        public string NamespaceName{get;set;}

        public string ServiceName{get;set;}

        public string[] ScanAssemblies{get;set;}

        public Assembly[] GetScanAssemblies()
        {
            var assemblies=new List<Assembly>();
            foreach(var item in ScanAssemblies)
            {
                assemblies.Add(Assembly.Load(item));
            }
            return assemblies.ToArray();
        }

    }
}
