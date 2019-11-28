using System;
using System.Collections.Generic;
using System.Reflection;

namespace Kadder.CodeGeneration
{
    public class RpcParameterInfo
    {
        public RpcParameterInfo()
        { }

        public RpcParameterInfo(ParameterInfo info)
        {
            Name = info.Name;
            ParameterType = info.ParameterType;
        }

        public string Name { get; set; }

        public Type ParameterType { get; set; }

        public bool IsEmpty { get; set; }

        public static List<RpcParameterInfo> Convert(ParameterInfo[] paramaters)
        {
            var rpcParams = new List<RpcParameterInfo>();
            foreach (var item in paramaters)
            {
                rpcParams.Add(new RpcParameterInfo(item));
            }
            return rpcParams;
        }
    }
}
