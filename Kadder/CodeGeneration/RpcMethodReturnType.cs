using System;
using System.Reflection;

namespace Kadder.CodeGeneration
{
    public class RpcMethodReturnType
    {
        public RpcMethodReturnType(Type type, bool isEmpty = false)
        {
            Name = type.Name;
            Namespace = type.Namespace;
            Assembly = type.Assembly;
            ReturnType = type;
            IsEmpty = isEmpty;
        }

        public string Name { get; set; }

        public Type ReturnType { get; set; }

        public string Namespace { get; set; }

        public Assembly Assembly { get; set; }

        public bool IsEmpty { get; set; }
    }
}
